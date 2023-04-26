using AutoFixture;
using Confluent.Kafka;
using Kafka.EventLoop.WorkerService.Models;
using Kafka.EventLoop.WorkerService.Produce;
using Microsoft.Extensions.Options;

namespace Kafka.EventLoop.WorkerService
{
    internal class MessageProduceService : BackgroundService
    {
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
                var fooProducer = CreateProducer<int, FooMessage>();
                var barProducer = CreateProducer<string, BarMessage>();

                var fooStream = StreamMessagesAsync(
                    fooProducer,
                    _settings.FooTopic,
                    x => x.Key,
                    stoppingToken);
                var barStream = StreamMessagesAsync(
                    barProducer,
                    _settings.BarTopic,
                    x => x.Key,
                    stoppingToken);

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
                .Build();
        }

        private async Task StreamMessagesAsync<TKey, TMessage>(
            IProducer<TKey, TMessage> producer,
            string topicName,
            Func<TMessage, TKey> keyProvider,
            CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var value = _fixture.Create<TMessage>();
                    var key = keyProvider(value);
                    var message = new Message<TKey, TMessage>
                    {
                        Key = key,
                        Value = value
                    };
                    await producer.ProduceAsync(topicName, message, stoppingToken);

                    _logger.LogDebug($"Published message {typeof(TMessage).Name} with key {key}");

                    var randomPause = 2000 + _fixture.Create<byte>() * 10;
                    await Task.Delay(randomPause, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}