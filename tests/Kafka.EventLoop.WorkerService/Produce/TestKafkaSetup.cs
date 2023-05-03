using Confluent.Kafka.Admin;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Kafka.EventLoop.WorkerService.Produce
{
    internal class TestKafkaSetup
    {
        private readonly TestSettings _settings;

        public TestKafkaSetup(IOptions<TestSettings> options)
        {
            _settings = options.Value;
        }

        public async Task EnsureKafkaTopicsAsync()
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = _settings.ConnectionString
            };
            using var adminClient = new AdminClientBuilder(config).Build();
            await EnsureTopicAsync(adminClient, _settings.FooTopic, _settings.FooTopicPartitionCount);
            await EnsureTopicAsync(adminClient, _settings.BarTopic, _settings.BarTopicPartitionCount);
            await EnsureTopicAsync(adminClient, _settings.BarDeadLettersTopic, _settings.BarDeadLettersTopicPartitionCount);
            await EnsureTopicAsync(adminClient, _settings.FooOneToOneStreamingTopic, _settings.FooOneToOneStreamingTopicPartitionCount);
        }

        public async Task DeleteKafkaTopicsAsync()
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = _settings.ConnectionString
            };
            using var adminClient = new AdminClientBuilder(config).Build();
            await DeleteTopicAsync(adminClient, _settings.FooTopic);
            await DeleteTopicAsync(adminClient, _settings.BarTopic);
            await DeleteTopicAsync(adminClient, _settings.BarDeadLettersTopic);
            await DeleteTopicAsync(adminClient, _settings.FooOneToOneStreamingTopic);
        }

        private static async Task EnsureTopicAsync(
            IAdminClient adminClient,
            string topicName,
            int numberOfPartitions)
        {
            try
            {
                await adminClient.CreateTopicsAsync(
                    new[]
                    {
                        new TopicSpecification
                        {
                            Name = topicName,
                            ReplicationFactor = 1,
                            NumPartitions = numberOfPartitions,
                            Configs = new Dictionary<string, string>
                            {
                                {"min.insync.replicas", "1"},
                                {"unclean.leader.election.enable", "false"}
                            }
                        }
                    },
                    new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(5) });
            }
            catch (CreateTopicsException)
            {
            }
        }

        private static async Task DeleteTopicAsync(
            IAdminClient adminClient,
            string topicName)
        {
            try
            {
                await adminClient.DeleteTopicsAsync(
                    new[] { topicName },
                    new DeleteTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(5) });
            }
            catch (DeleteTopicsException)
            {
            }
        }
    }
}
