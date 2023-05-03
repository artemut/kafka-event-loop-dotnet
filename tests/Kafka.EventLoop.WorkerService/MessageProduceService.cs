using AutoFixture;
using Confluent.Kafka;
using Kafka.EventLoop.WorkerService.Models;
using Kafka.EventLoop.WorkerService.Produce;
using Microsoft.Extensions.Options;

namespace Kafka.EventLoop.WorkerService
{
    internal class MessageProduceService : BackgroundService
    {
        // 1 - foo only
        // 2 - bar only
        // 3 - both
        private const int SendType = 1;
        private const int MaxMessageCount = 10;

        private readonly IFixture _fixture;
        private readonly TestSettings _settings;
        private readonly ILogger<MessageProduceService> _logger;

        public MessageProduceService(
            IOptions<TestSettings> options,
            ILogger<MessageProduceService> logger)
        {
            _fixture = new Fixture();
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(5000, stoppingToken);

                var fooProducer = CreateProducer<int, FooMessage>();
                var barProducer = CreateProducer<string, BarMessage>();

                var fooStream = SendType != 2 ? StreamMessagesAsync(
                    fooProducer,
                    _settings.FooTopic,
                    x => x,
                    stoppingToken) : Task.CompletedTask;
                var barStream = SendType != 1 ? StreamMessagesAsync(
                    barProducer,
                    _settings.BarTopic,
                    x => x.ToString(),
                    stoppingToken) : Task.CompletedTask;

                await Task.WhenAll(fooStream, barStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
            }
        }

        private IProducer<TKey, TMessage> CreateProducer<TKey, TMessage>()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _settings.ConnectionString,
                EnableDeliveryReports = false
            };
            return new ProducerBuilder<TKey, TMessage>(config)
                .SetValueSerializer(new JsonSerializer<TMessage>())
                .SetLogHandler((_, _) => { })
                .Build();
        }

        private async Task StreamMessagesAsync<TKey, TMessage>(
            IProducer<TKey, TMessage> producer,
            string topicName,
            Func<int, TKey> keyConverter,
            CancellationToken stoppingToken)
            where TMessage : IMessage<TKey>
        {
            try
            {
                _logger.LogInformation($"Start publishing messages {typeof(TMessage).Name}");
                var incrementalKey = 0;
                while (!stoppingToken.IsCancellationRequested && ++incrementalKey <= MaxMessageCount)
                {
                    var key = keyConverter(incrementalKey);
                    var value = _fixture
                        .Build<TMessage>()
                        .With(x => x.Key, key)
                        .Create();
                    var message = new Message<TKey, TMessage>
                    {
                        Key = key,
                        Value = value
                    };
                    await producer.ProduceAsync(topicName, message, stoppingToken);

                    _logger.LogDebug($"Published message {typeof(TMessage).Name} with key {key}");
                    
                    var randomPause = _fixture.Create<byte>() / 10;
                    await Task.Delay(randomPause, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}