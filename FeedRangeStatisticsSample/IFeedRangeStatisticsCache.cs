using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedRangeStatisticsSample
{
    public interface IFeedRangeStatisticsCache : IDisposable
    {
        public Container Container { get; }

        public string ContainerResourceId { get; }

        public DateTimeOffset LastUpdated { get; }

        public bool TryGetStatistics(FeedRange logicalPartitionFeedRange, out FeedRangeStatistics statistics);

        public bool TryGetStatistics(PartitionKey logicalPartitionKeyValue, out FeedRangeStatistics statistics);
    }
}
