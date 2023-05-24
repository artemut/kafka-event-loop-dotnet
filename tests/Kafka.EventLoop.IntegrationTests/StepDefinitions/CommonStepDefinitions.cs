namespace Kafka.EventLoop.IntegrationTests.StepDefinitions
{
    [Binding]
    public class CommonStepDefinitions
    {
        [When("wait for (\\d+) second\\(s\\)")]
        public async Task WaitAsync(int seconds)
        {
            await Task.Delay(seconds * 1000);
        }
    }
}
