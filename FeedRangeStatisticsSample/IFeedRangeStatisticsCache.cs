using Microsoft.Azure.Cosmos;
using System;

namespace FeedRangeStatisticsSample
{
    public interface IFeedRangeStatisticsCache : IDisposable
    {
        public Container Container { get; }

        public string ContainerResourceId { get; }

        public DateTimeOffset LastUpdated { get; }

        public bool TryGetStatistics(PartitionKey logicalPartitionKeyValue, out FeedRangeStatistics statistics);
    }
}
