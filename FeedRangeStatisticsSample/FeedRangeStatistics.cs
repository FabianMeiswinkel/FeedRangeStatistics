using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeedRangeStatisticsSample
{
    public class FeedRangeStatistics
    {
        public FeedRangeStatistics(FeedRange logicalPartitionFeedRange, long sizeInKB)
        {
            if (logicalPartitionFeedRange == null)
            {
                throw new ArgumentNullException(nameof(logicalPartitionFeedRange));
            }

            if (sizeInKB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInKB));
            }

            this.LogicalPartitionFeedRange = logicalPartitionFeedRange;
            this.SizeInKB = sizeInKB;
        }

        public FeedRange LogicalPartitionFeedRange { get; }

        public long SizeInKB { get; }
    }
}
