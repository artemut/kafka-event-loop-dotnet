using System.Collections.Concurrent;

namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions
{
    internal static class DictionaryExtensions
    {
        public static void AddOrAppend<TKey, TListItem>(
            this ConcurrentDictionary<TKey, List<TListItem>> dictionary,
            TKey key,
            IEnumerable<TListItem> items) where TKey : notnull
        {
            dictionary.AddOrUpdate(
                key,
                _ => items.ToList(),
                (_, existingList) => existingList.Concat(items).ToList());
        }

        public static void AddOrAppend<TKey, TListItem>(
            this Dictionary<TKey, List<TListItem>> dictionary,
            TKey key,
            IEnumerable<TListItem> items) where TKey : notnull
        {
            if (!dictionary.TryGetValue(key, out var currentItems))
            {
                currentItems = new List<TListItem>();
                dictionary.Add(key, currentItems);
            }
            currentItems.AddRange(items);
        }
    }
}
