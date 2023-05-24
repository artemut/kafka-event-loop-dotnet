using Kafka.EventLoop.IntegrationTests.Infrastructure.Hosting;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Kafka;
using Kafka.EventLoop.IntegrationTests.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure
{
    internal class TestSession : IDisposable
    {
        private static readonly object Lock = new();
        private static volatile TestSession? _current;

        public static TestSession Current
        {
            get
            {
                if (_current != null) return _current;
                lock (Lock)
                {
                    if (_current != null) return _current;
                    _current = new TestSession();
                }
                return _current;
            }
        }

        private TestSession()
        {
            Config = new TestSetupConfig();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("settings.json")
                .Build();
            configuration.GetSection("TestSetup").Bind(Config);

            KafkaHelper = new KafkaHelper(Config);
            EventsInterceptor = new EventsInterceptor();
            HostHelper = new HostHelper(EventsInterceptor);
            ExpectedConsumerIds = CalculateExpectedConsumerIds(configuration);
        }

        public IFixture Fixture => new Fixture();
        public TestSetupConfig Config { get; }
        public KafkaHelper KafkaHelper { get; }
        public EventsInterceptor EventsInterceptor { get; }
        public HostHelper HostHelper { get; }
        public ConsumerId[] ExpectedConsumerIds { get; }

        public async Task EnsureConsumersAreSubscribedAsync()
        {
            var timeout = TimeSpan.FromSeconds(15);
            var cts = new CancellationTokenSource(timeout);
            while (!cts.IsCancellationRequested)
            {
                if (EventsInterceptor.AreConsumersSubscribed(ExpectedConsumerIds))
                {
                    return;
                }
                await Task.Delay(500, CancellationToken.None);
            }
            throw new Exception($"Some or all consumers are still not subscribed after configured timeout: {timeout}");
        }

        public void Dispose()
        {
            KafkaHelper.Dispose();
            _current = null;
        }

        private ConsumerId[] CalculateExpectedConsumerIds(IConfiguration configuration)
        {
            var expectedConsumerIds = configuration
                .GetSection("Kafka")
                .GetSection("ConsumerGroups")
                .GetChildren()
                .Select(c => new
                {
                    GroupId = c["GroupId"]!,
                    ParallelConsumers = int.Parse(c["ParallelConsumers"]!)
                })
                .SelectMany(x => Enumerable
                    .Range(0, x.ParallelConsumers)
                    .Select(i => new ConsumerId(x.GroupId, i)))
                .ToArray();

            if (!expectedConsumerIds.Any())
            {
                throw new InvalidOperationException("No consumers found");
            }

            return expectedConsumerIds;
        }
    }
}
