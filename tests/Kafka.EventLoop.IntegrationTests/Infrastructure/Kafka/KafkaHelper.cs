using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Models;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Settings;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka
{
    internal class KafkaHelper : IDisposable
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(5);

        private readonly TestSetupConfig _config;
        private readonly IAdminClient _adminClient;
        private readonly IProducer<Ignore, ProductOrderModel> _productOrdersProducer;

        public KafkaHelper(TestSetupConfig config)
        {
            _config = config;
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = _config.ConnectionString
            };
            _adminClient = new AdminClientBuilder(adminConfig).Build();

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.ConnectionString,
                EnableDeliveryReports = true,
                Acks = Acks.All
            };
            _productOrdersProducer = new ProducerBuilder<Ignore, ProductOrderModel>(producerConfig)
                .SetKeySerializer(new IgnoreSerializer())
                .SetValueSerializer(new JsonSerializer<ProductOrderModel>())
                .Build();
        }

        public async Task CreateTopicsAsync()
        {
            foreach (var topicConfig in _config.Topics)
            {
                var specification = new TopicSpecification
                {
                    Name = topicConfig.Name,
                    ReplicationFactor = 1,
                    NumPartitions = topicConfig.NumberOfPartitions,
                    Configs = new Dictionary<string, string>
                    {
                        { "min.insync.replicas", "1" },
                        { "unclean.leader.election.enable", "false" }
                    }
                };
                await _adminClient.CreateTopicsAsync(
                    new[] { specification },
                    new CreateTopicsOptions { RequestTimeout = RequestTimeout });
            }
        }

        public async Task<ProducedItem[]> ProduceAsync(
            string topicName,
            int partition,
            ProductOrderModel[] messages)
        {
            var items = new List<ProducedItem>();
            foreach (var message in messages)
            {
                var result = await _productOrdersProducer
                    .ProduceAsync(
                        new TopicPartition(topicName, partition),
                        new Message<Ignore, ProductOrderModel> { Value = message });
                if (result.Status != PersistenceStatus.Persisted)
                    throw new Exception("Message is not persisted");
                items.Add(new ProducedItem(partition, message, result.Timestamp.UtcDateTime));
            }
            return items.ToArray();
        }

        public async Task<ProducedItem[]> ProduceInParallelAsync(
            string topicName,
            int partition,
            ProductOrderModel[] messages)
        {
            var tasks = messages.Select(message => _productOrdersProducer
                .ProduceAsync(
                    new TopicPartition(topicName, partition),
                    new Message<Ignore, ProductOrderModel> { Value = message }));
            var results = await Task.WhenAll(tasks);
            return results
                .Select(r => new ProducedItem(partition, r.Value, r.Timestamp.UtcDateTime))
                .OrderBy(x => x.ProducedAt)
                .ToArray();
        }

        public async Task DeleteTopicsAsync()
        {
            foreach (var topicConfig in _config.Topics)
            {
                await _adminClient.DeleteTopicsAsync(
                    new[] { topicConfig.Name },
                    new DeleteTopicsOptions { RequestTimeout = RequestTimeout });
            }
        }

        public async Task<List<string>> GetActiveConsumersAsync(string topicName)
        {
            var allConsumerGroups = await _adminClient.ListConsumerGroupsAsync(
                new ListConsumerGroupsOptions { RequestTimeout = RequestTimeout });
            var groupIds = allConsumerGroups.Valid.Select(cg => cg.GroupId).ToArray();

            var results = await _adminClient.DescribeConsumerGroupsAsync(
                groupIds,
                new DescribeConsumerGroupsOptions { RequestTimeout = RequestTimeout });

            return results.ConsumerGroupDescriptions
                .SelectMany(cg => cg.Members)
                .Where(m => m.Assignment.TopicPartitions.Any(tp => tp.Topic == topicName))
                .Select(m => m.ConsumerId)
                .ToList();
        }

        public void Dispose()
        {
            _productOrdersProducer.Dispose();
            _adminClient.Dispose();
        }
    }
}
