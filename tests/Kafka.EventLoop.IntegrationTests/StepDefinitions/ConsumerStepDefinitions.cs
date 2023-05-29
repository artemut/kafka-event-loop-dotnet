using System.Collections.Concurrent;
using Kafka.EventLoop.IntegrationTests.Infrastructure;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;
using Kafka.EventLoop.Streaming;

namespace Kafka.EventLoop.IntegrationTests.StepDefinitions
{
    [Binding]
    public class ConsumerStepDefinitions
    {
        private readonly ConcurrentQueue<ProducedItem> _produceHistory = new();
        private readonly List<OneToOneLink<ProductOrderModel, ProductOrderExtendedModel>> _streamed = new();
        private string _topicName = null!;
        private string _groupId = null!;
        private DateTime _lastPublishStarted;

        [Given("topic (.*)")]
        public void Topic(string topicName)
        {
            _topicName = topicName;
        }

        [Given("consumer group (.*)")]
        public void ConsumerGroup(string groupId)
        {
            _groupId = groupId;
        }

        [When("partition (\\d+) receives (\\d+) product order")]
        [When("partition (\\d+) receives (\\d+) product orders")]
        public async Task PartitionReceivesProductOrdersAsync(int partition, int count)
        {
            var productOrders = TestSession.Current.Fixture.CreateMany<ProductOrderModel>(count).ToArray();
            _lastPublishStarted = DateTime.Now;
            var items = await TestSession.Current.KafkaHelper.ProduceAsync(_topicName, partition, productOrders);
            _produceHistory.Enqueue(items);
        }

        [When("partitions gradually receive product orders for the duration of (\\d+) seconds:")]
        public async Task PartitionsGraduallyReceiveProductOrdersAsync(int durationInSec, Table partitionsTable)
        {
            var fixture = TestSession.Current.Fixture;
            _lastPublishStarted = DateTime.Now;
            var timers = partitionsTable
                .AsGradualProduce()
                .Select(x =>
                    new BlockingTimer(async () =>
                        {
                            var productOrders = fixture
                                .Build<ProductOrderModel>()
                                .With(y => y.OrderedAt, () => DateTimeOffset.UtcNow)
                                .CreateMany(x.MessagesPerSecond)
                                .ToArray();
                            var items = await TestSession.Current.KafkaHelper
                                .ProduceAsync(_topicName, x.Partition, productOrders);
                            _produceHistory.Enqueue(items);
                        },
                        TimeSpan.FromSeconds(1)))
                .ToList();

            await Task.Delay(TimeSpan.FromSeconds(durationInSec));
            timers.ForEach(t => t.Dispose());
        }

        [When("partitions receive product orders with given ids:")]
        public async Task PartitionsReceiveProductOrdersAsync(Table partitionsTable)
        {
            var fixture = TestSession.Current.Fixture;
            _lastPublishStarted = DateTime.Now;
            var produceTasks = partitionsTable
                .AsProduceWithGivenIds()
                .GroupBy(x => x.Partition)
                .Select(async g =>
                {
                    var productOrders = g.Select(x => fixture
                        .Build<ProductOrderModel>()
                        .With(y => y.Id, x.Id)
                        .Create()).ToArray();
                    var items = await TestSession.Current.KafkaHelper.ProduceAsync(_topicName, g.Key, productOrders);
                    _produceHistory.Enqueue(items);
                })
                .ToList();

            await Task.WhenAll(produceTasks);
        }

        [When("partitions receive product orders in parallel:")]
        public async Task PartitionsReceiveProductOrdersInParallelAsync(Table partitionsTable)
        {
            var fixture = TestSession.Current.Fixture;
            _lastPublishStarted = DateTime.Now;
            var produceTasks = partitionsTable
                .AsProduce()
                .Select(async x =>
                {
                    var productOrders = fixture
                        .CreateMany<ProductOrderModel>(x.Count)
                        .ToArray();
                    var items = await TestSession.Current.KafkaHelper
                        .ProduceInParallelAsync(_topicName, x.Partition, productOrders);
                    _produceHistory.Enqueue(items);
                })
                .ToList();

            await Task.WhenAll(produceTasks);
        }

        [Then("no product order was consumed")]
        public void NoProductOrderWasConsumed()
        {
            var receivedProductOrders = TestSession.Current.EventsInterceptor.GetReceivedProductOrders(_groupId);
            receivedProductOrders.Should().BeNull();

            var actualTimes = TestSession.Current.EventsInterceptor.GetControllerCreatedCount(_groupId);
            actualTimes.Should().Be(0);
        }

        [Then("product orders were consumed:")]
        public void ProductOrdersWereConsumed(Table table)
        {
            var expectedProductOrders = new Dictionary<int, List<ProductOrderModel>>();
            foreach (var expectation in table.AsConsume())
            {
                var productOrders = _produceHistory
                    .Where(x => x.Partition == expectation.Partition)
                    .Select(x => x.ProductOrder)
                    .Skip(expectation.Skip)
                    .Take(expectation.Take)
                    .ToList()
                    .Duplicate(expectation.Times);
                expectedProductOrders.AddOrAppend(expectation.Partition, productOrders);
            }

            var receivedProductOrders = TestSession.Current.EventsInterceptor.GetReceivedProductOrders(_groupId);
            receivedProductOrders.Should().NotBeNull();
            var actualProductOrders = receivedProductOrders!
                .GroupBy(x => x.Partition)
                .ToDictionary(x => x.Key, x => x.Select(m => m.Value).ToList());

            AssertMessages(actualProductOrders, expectedProductOrders);
        }

        [Then("product orders with given ids were consumed:")]
        public void ProductOrdersWithGivenIdsWereConsumed(Table table)
        {
            var expectedProductOrders = new Dictionary<int, List<ProductOrderModel>>();
            foreach (var expectation in table.AsConsumeWithGivenIds())
            {
                var productOrders = _produceHistory
                    .Where(x => x.Partition == expectation.Partition)
                    .Select(x => x.ProductOrder)
                    .Where(x => expectation.Ids.Contains(x.Id))
                    .ToList();
                expectedProductOrders.AddOrAppend(expectation.Partition, productOrders);
            }

            var receivedProductOrders = TestSession.Current.EventsInterceptor.GetReceivedProductOrders(_groupId);
            receivedProductOrders.Should().NotBeNull();
            var actualProductOrders = receivedProductOrders!
                .GroupBy(x => x.Partition)
                .ToDictionary(x => x.Key, x => x.Select(m => m.Value).ToList());

            AssertMessages(actualProductOrders, expectedProductOrders);
        }

        [Then("consumption happens at maximum rate (\\d+) tps during the period of (\\d+) seconds")]
        public async Task ConsumptionHappensAtGiveRate(int tps, int totalDurationInSec)
        {
            var counts = new List<(DateTime At, int Count)>
            {
                (_lastPublishStarted, 0)
            };
            var cts = new CancellationTokenSource();
            var timerStarted = false;
            while (!cts.IsCancellationRequested)
            {
                var currentTime = DateTime.Now;
                var receivedProductOrders = TestSession.Current.EventsInterceptor.GetReceivedProductOrders(_groupId);
                var count = receivedProductOrders?.Length ?? 0;
                if (count > 0)
                {
                    counts.Add((currentTime, count));
                    if (!timerStarted)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(totalDurationInSec));
                        timerStarted = true;
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            }
            
            var rates = ConvertToTpsRates(counts);
            rates.Average().Should().BeLessThanOrEqualTo(tps);
            const double allowedError = 0.01;
            var tpsAdjusted = tps * (1 + allowedError);
            rates.Should().AllSatisfy(rate => rate.Should().BeLessThanOrEqualTo(tpsAdjusted));
        }

        [Then("controller was invoked (\\d+) time")]
        [Then("controller was invoked (\\d+) times")]
        public void ControllerWasInvoked(int times)
        {
            var actualCallTimes = TestSession.Current.EventsInterceptor.GetControllerCreatedCount(_groupId);
            actualCallTimes.Should().Be(times);
            TestSession.Current.EventsInterceptor.Reset(_groupId);
        }

        [Then("controller was invoked at least (\\d+) times, at most (\\d+) times")]
        public void ControllerWasInvoked(int minTimes, int maxTimes)
        {
            var actualCallTimes = TestSession.Current.EventsInterceptor.GetControllerCreatedCount(_groupId);
            actualCallTimes.Should().BeInRange(minTimes, maxTimes);
            TestSession.Current.EventsInterceptor.Reset(_groupId);
        }

        [Then("topic has no consumers")]
        public async Task TopicHasNoConsumersAsync()
        {
            var activeConsumers = await TestSession.Current.KafkaHelper.GetActiveConsumersAsync(_topicName);
            activeConsumers.Should().BeEmpty();
        }

        [Then("extended product orders were streamed:")]
        public void ExtendedProductOrdersWereStreamed(Table table)
        {
            var expectedProductOrders = new Dictionary<int, List<ProductOrderModel>>();
            foreach (var expectation in table.AsConsume())
            {
                var productOrders = _produceHistory
                    .Where(x => x.Partition == expectation.Partition)
                    .Select(x => x.ProductOrder)
                    .Skip(expectation.Skip)
                    .Take(expectation.Take)
                    .ToList();
                expectedProductOrders.AddOrAppend(expectation.Partition, productOrders);
            }

            var oneToOneLinks = TestSession.Current.EventsInterceptor.GetReceivedOneToOneStreamingLinks(_groupId);
            oneToOneLinks.Should().NotBeNullOrEmpty();
            var incomingProductOrders = oneToOneLinks!
                .Select(x => x.IncomingMessage)
                .GroupBy(x => x.Partition)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Value).ToList());

            AssertMessages(incomingProductOrders, expectedProductOrders);
            
            _streamed.AddRange(oneToOneLinks!);
        }

        [Then("extended product orders were consumed:")]
        public void ExtendedProductOrdersWereConsumed(Table table)
        {
            var expectedProductOrders = new Dictionary<int, List<ProductOrderExtendedModel>>();
            foreach (var expectation in table.AsConsume())
            {
                var productOrders = _streamed
                    .Where(x => x.IncomingMessage.Partition == expectation.Partition)
                    .Select(x => x.OutgoingMessage)
                    .Skip(expectation.Skip)
                    .Take(expectation.Take)
                    .ToList()
                    .Duplicate(expectation.Times);
                expectedProductOrders.AddOrAppend(expectation.Partition, productOrders);
            }

            var receivedProductOrders = TestSession.Current.EventsInterceptor.GetReceivedProductOrdersExtended(_groupId);
            receivedProductOrders.Should().NotBeNull();
            var actualProductOrders = receivedProductOrders!
                .GroupBy(x => x.Partition)
                .ToDictionary(x => x.Key, x => x.Select(m => m.Value).ToList());

            AssertMessages(actualProductOrders, expectedProductOrders);
        }

        private static void AssertMessages<T>(
            Dictionary<int, List<T>> actual,
            Dictionary<int, List<T>> expected)
            where T : notnull
        {
            // verify partitions
            var actualPartitions = actual.Keys;
            var expectedPartitions = expected.Keys;
            actualPartitions.Should().BeEquivalentTo(expectedPartitions, o => o.WithoutStrictOrdering());

            // verify order of messages
            var actualItems = BuildItemsRepresentation(actual);
            var expectedItems = BuildItemsRepresentation(expected);
            actualItems.Should().Be(expectedItems);

            // verify message fields
            foreach (var partition in expectedPartitions)
            {
                var actualPartitionItems = actual[partition];
                var expectedPartitionItems = expected[partition];
                actualPartitionItems.Should().BeEquivalentTo(expectedPartitionItems, o => o.WithStrictOrdering());
            }
        }

        private static string BuildItemsRepresentation<T>(IDictionary<int, List<T>> dictionaryOfItems)
            where T : notnull
        {
            var partitions = dictionaryOfItems
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => $"{kvp.Key}: {BuildItemsRepresentation(kvp.Value)}");
            return $"{Environment.NewLine}{string.Join(Environment.NewLine, partitions)}{Environment.NewLine}";
        }

        private static string BuildItemsRepresentation<T>(List<T> items)
            where T : notnull
        {
            return $"[{items.Count}] {string.Join(",", items.Select(x => x.ToString()))}";
        }

        private static List<double> ConvertToTpsRates(List<(DateTime At, int Count)> counts)
        {
            var rates = new List<double>();
            for (var i = 1; i < counts.Count; i++)
            {
                var prev = counts[i - 1];
                var current = counts[i];
                var interval = (current.At - prev.At).TotalSeconds;
                var count = current.Count - prev.Count;
                var rate = count / interval;
                rates.Add(rate);
            }
            // first 2 rates should be averaged
            if (rates.Count >= 2)
            {
                rates[1] = (rates[0] + rates[1]) / 2;
                rates = rates.Skip(1).ToList();
            }
            return rates;
        }
    }
}
