using Kafka.EventLoop.Configuration.ConfigTypes;
using Kafka.EventLoop.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaBackgroundService : BackgroundService
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly IKafkaWorkerFactory _kafkaWorkerFactory;
        private readonly ILogger<KafkaBackgroundService> _logger;

        public KafkaBackgroundService(
            KafkaConfig kafkaConfig,
            IKafkaWorkerFactory kafkaWorkerFactory,
            ILogger<KafkaBackgroundService> logger)
        {
            _kafkaConfig = kafkaConfig;
            _kafkaWorkerFactory = kafkaWorkerFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workers = new List<IKafkaWorker>();
            foreach (var consumerGroup in _kafkaConfig.ConsumerGroups)
            {
                _logger.LogInformation(
                    $"Starting {consumerGroup.ParallelConsumers} consumers for consumer group {consumerGroup.Name}");

                for (var i = 0; i < consumerGroup.ParallelConsumers; i++)
                {
                    var worker = _kafkaWorkerFactory.Create(consumerGroup.Name, i);
                    workers.Add(worker);
                }
            }

            try
            {
                var tasks = workers.Select(c => Task.Run(() => c.RunAsync(stoppingToken), stoppingToken));
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
            }
        }
    }
}
