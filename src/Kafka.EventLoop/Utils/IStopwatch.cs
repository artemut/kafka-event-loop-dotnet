namespace Kafka.EventLoop.Utils
{
    internal interface IStopwatch
    {
        void Start();
        TimeSpan Stop();
    }
}
