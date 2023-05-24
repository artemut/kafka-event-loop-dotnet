using System.Collections.Concurrent;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions
{
    internal static class ConcurrentQueueExtensions
    {
        public static void Enqueue<T>(this ConcurrentQueue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}
