using Kafka.EventLoop.WorkerService.Models;

namespace Kafka.EventLoop.WorkerService.Custom
{
    internal class FooIntakeObserver : IKafkaIntakeObserver<FooMessage>
    {
    }
}
