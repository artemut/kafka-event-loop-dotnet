using Kafka.EventLoop.IntegrationTests.Infrastructure;

namespace Kafka.EventLoop.IntegrationTests.StepDefinitions
{
    [Binding]
    public class InfrastructureStepDefinitions
    {
        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            TestSession.Current.KafkaHelper.CreateTopicsAsync().GetAwaiter().GetResult();
            TestSession.Current.HostHelper.Start();
            TestSession.Current.EnsureConsumersAreSubscribedAsync().GetAwaiter().GetResult();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            TestSession.Current.KafkaHelper.DeleteTopicsAsync().GetAwaiter().GetResult();
            TestSession.Current.HostHelper.Stop();
            TestSession.Current.Dispose();
        }
    }
}
