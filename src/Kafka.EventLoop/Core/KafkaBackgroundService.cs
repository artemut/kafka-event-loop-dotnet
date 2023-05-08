using Kafka.EventLoop.Configuration.ConfigTypes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kafka.EventLoop.Core
{
    internal class KafkaBackgroundService : BackgroundService
    {
        private readonly KafkaConfig _kafkaConfig;
        private readonly Func<string, string, IKafkaWorker> _kafkaWorkerFactory;
        private readonly ILogger<KafkaBackgroundService> _logger;

        public KafkaBackgroundService(
            KafkaConfig kafkaConfig,
            Func<string, string, IKafkaWorker> kafkaWorkerFactory,
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
                    "Starting {ParallelConsumers} consumers for consumer group {GroupId}",
                    consumerGroup.ParallelConsumers,
                    consumerGroup.GroupId);

                for (var i = 0; i < consumerGroup.ParallelConsumers; i++)
                {
                    var consumerName = $"{consumerGroup.GroupId}:{i}";
                    var worker = _kafkaWorkerFactory(consumerGroup.GroupId, consumerName);
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
