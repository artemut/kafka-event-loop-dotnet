namespace Kafka.EventLoop.IntegrationTests.Infrastructure.Extensions
{
    internal static class TableExtensions
    {
        public static (int Partition, int Count)[] AsProduce(this Table table)
        {
            return table.Rows
                .Select(row =>
                (
                    int.Parse(row["Partition"]),
                    int.Parse(row["Count"])
                ))
                .ToArray();
        }

        public static (int Partition, int MessagesPerSecond)[] AsGradualProduce(this Table table)
        {
            return table.Rows
                .Select(row =>
                (
                    int.Parse(row["Partition"]),
                    int.Parse(row["Messages per second"])
                ))
                .ToArray();
        }

        public static (int Partition, long Id)[] AsProduceWithGivenIds(this Table table)
        {
            return table.Rows
                .Select(row =>
                (
                    int.Parse(row["Partition"]),
                    long.Parse(row["Id"])
                ))
                .ToArray();
        }

        public static (int Partition, int Skip, int Take, int Times)[] AsConsume(this Table table)
        {
            return table.Rows
                .Select(row =>
                (
                    int.Parse(row["Partition"]),
                    int.Parse(row["Skip"]),
                    int.Parse(row["Take"]),
                    row.ContainsKey("Times") ? int.Parse(row["Times"]) : 1
                ))
                .ToArray();
        }

        public static (int Partition, long[] Ids)[] AsConsumeWithGivenIds(this Table table)
        {
            return table.Rows
                .Select(row =>
                (
                    int.Parse(row["Partition"]),
                    row["Ids"].Split(",", StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray()
                ))
                .ToArray();
        }
    }
}
