using Microsoft.Azure.Cosmos;
using System;

namespace FeedRangeStatisticsSample
{
    public interface IFeedRangeStatisticsCache : IDisposable
    {
        /// <summary>
        /// Gets the container for the feed range statistics cache
        /// </summary>
        public Container Container { get; }

        /// <summary>
        /// Gets the unique Collection resource id to identify when collections 
        /// get deleted and recreated with the same name
        /// </summary>
        public string ContainerResourceId { get; }

        /// <summary>
        /// The timestamp of the last time the statistics were retrieved from the service
        /// </summary>
        public DateTimeOffset LastUpdated { get; }

        /// <summary>
        /// Attempts to get the statistics for a logical partition. An entry will only
        /// be returned if the logical partition has been sampled (exceeding a certain storage
        /// limit and being under the "worst offenders" on its physical partition)
        /// </summary>
        /// <param name="logicalPartitionKeyValue">The logical parition key value</param>
        /// <param name="statistics"></param>
        /// <returns></returns>
        public bool TryGetStatistics(
            PartitionKey logicalPartitionKeyValue,
            out FeedRangeStatistics statistics);
    }
}
