namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions
{
    internal static class ListExtensions
    {
        public static List<T> Duplicate<T>(this List<T> list, int times)
        {
            var length = list.Count;
            var arr = new T[length * times];
            for (var i = 0; i < length; i++)
            {
                var item = list[i];
                for (var j = 0; j < times; j++)
                {
                    arr[i + length * j] = item;
                }
            }
            return arr.ToList();
        }
    }
}
