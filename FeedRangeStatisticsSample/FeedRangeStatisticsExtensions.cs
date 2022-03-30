using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace FeedRangeStatisticsSample
{
    public static class FeedRangeStatisticsExtensions
    {
        public static async Task<IFeedRangeStatisticsCache> CreateFeedRangeStatisticsCache(
            this Container container,
            TimeSpan refreshInterval,            
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (container == null) {
                throw new ArgumentNullException(nameof(container));
            }

            using var response = await container.ReadContainerStreamAsync(
                new ContainerRequestOptions
                {
                    AddRequestHeaders = headers =>
                    {
                        headers.Add("x-ms-documentdb-populatepartitionstatistics", "true");
                        headers.Add("x-ms-cosmos-internal-get-all-partition-key-stats", "true");
                    }
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var jsonDocument = await JsonDocument.ParseAsync(response.Content);
            List<FeedRangeStatistics> statistics = ParseStatistics(jsonDocument);

            return new FeedRangeStatisticsCache(
                container,
                jsonDocument.RootElement.GetProperty("_rid").GetString(),
                statistics,
                refreshInterval,
                logger);
        }

        private static List<FeedRangeStatistics> ParseStatistics(JsonDocument jsonDocument)
        {
            List<FeedRangeStatistics> statistics = new List<FeedRangeStatistics>();
            if (jsonDocument.RootElement.TryGetProperty("statistics", out JsonElement statisticsJson))
            {
                Trace.Assert(statisticsJson.ValueKind == JsonValueKind.Array);
                foreach (JsonElement physicalPartitionStatistics in statisticsJson.EnumerateArray())
                {
                    if (physicalPartitionStatistics.TryGetProperty(
                        "partitionKeys", out JsonElement logicalPartitionStatisticsArrayJson))
                    {
                        Trace.Assert(logicalPartitionStatisticsArrayJson.ValueKind == JsonValueKind.Array);
                        foreach (JsonElement logicalPartitionStatisticsJson in
                            logicalPartitionStatisticsArrayJson.EnumerateArray())
                        {
                            string feedRangeJson = $"{{\"PK\":\"{logicalPartitionStatisticsJson.GetProperty("partitionKey").GetRawText().Replace("\"", "\\\"")}\"}}";
                            statistics.Add(new FeedRangeStatistics(
                                FeedRange.FromJsonString(
                                    feedRangeJson),
                                logicalPartitionStatisticsJson.GetProperty("sizeInKB").GetInt64()));
                        }
                    }
                }
            }

            return statistics;
        }

        private class FeedRangeStatisticsCache : IFeedRangeStatisticsCache
        {
            private readonly TimeSpan refreshInterval;
            private readonly ILogger logger;
            private Dictionary<string, FeedRangeStatistics> sampledPartitionStatistics;
            private bool disposedValue;
            private Exception exceptionToBeRethrown;

            public FeedRangeStatisticsCache(
                Container container,
                string rid,
                IEnumerable<FeedRangeStatistics> sampledPartitionStatistics,
                TimeSpan refreshInterval,
                ILogger logger)
            {
                this.Container = container;
                this.ContainerResourceId = rid;
                this.LastUpdated = DateTimeOffset.UtcNow;
                this.sampledPartitionStatistics = 
                    sampledPartitionStatistics.ToDictionary(statistics => statistics.LogicalPartitionFeedRange.ToJsonString());
                this.refreshInterval = refreshInterval;
                this.logger = logger;

                if (refreshInterval < TimeSpan.MaxValue)
                {
                    this.StartRefreshLoop();
                }
            }

            public Container Container { get; private set; }

            public string ContainerResourceId { get; private set; }

            public DateTimeOffset LastUpdated { get; private set; }

            public bool TryGetStatistics(
                FeedRange logicalPartitionFeedRange,
                out FeedRangeStatistics statistics)
            {
                if (this.exceptionToBeRethrown != null)
                {
                    throw this.exceptionToBeRethrown;
                }

                Dictionary<String, FeedRangeStatistics> snapshot = sampledPartitionStatistics;
                return snapshot.TryGetValue(logicalPartitionFeedRange.ToJsonString(), out statistics);
            }

            public bool TryGetStatistics(
                PartitionKey logicalPartitionKeyValue,
                out FeedRangeStatistics statistics)
            {
                if (this.exceptionToBeRethrown != null)
                {
                    throw this.exceptionToBeRethrown;
                }

                Dictionary<String, FeedRangeStatistics> snapshot = sampledPartitionStatistics;
                return snapshot.TryGetValue(FeedRange.FromPartitionKey(logicalPartitionKeyValue).ToJsonString(), out statistics);
            }

            private async void StartRefreshLoop()
            {
                while (!this.disposedValue)
                {
                    try
                    {
                        await Task.Delay(this.refreshInterval);

                        if (!this.disposedValue)
                        {
                            using var response = await this.Container.ReadContainerStreamAsync(
                                new ContainerRequestOptions
                                {
                                    AddRequestHeaders = headers =>
                                    {
                                        headers.Add("x-ms-documentdb-populatepartitionstatistics", "true");
                                        headers.Add("x-ms-cosmos-internal-get-all-partition-key-stats", "true");
                                    }
                                },
                                CancellationToken.None);

                            if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                this.exceptionToBeRethrown = new InvalidOperationException(
                                    $"Container '{this.ContainerResourceId}' was deleted and recreated.");

                                break;
                            }

                            response.EnsureSuccessStatusCode();

                            using var jsonDocument = await JsonDocument.ParseAsync(response.Content);

                            if (this.ContainerResourceId != jsonDocument.RootElement.GetProperty("_rid").GetString())
                            {
                                this.exceptionToBeRethrown = new InvalidOperationException(
                                    $"Container '{this.ContainerResourceId}' was deleted and recreated.");

                                break;
                            }

                            List<FeedRangeStatistics> statistics = ParseStatistics(jsonDocument);
                            this.sampledPartitionStatistics =
                                statistics.ToDictionary(statistics => statistics.LogicalPartitionFeedRange.ToJsonString());

                            logger.LogInformation(
                                "{0}|{1}|{2}({3}): Refreshed statistics successfully",
                                this.Container.Database.Client.Endpoint,
                                this.Container.Database.Id,
                                this.Container.Id,
                                this.ContainerResourceId);
                        }
                    } 
                    catch(Exception error)
                    {
                        if (this.logger != null)
                        {
                            logger.LogWarning("{0}|{1}|{2}({3}): Refresh failed: {4}",
                                this.Container.Database.Client.Endpoint,
                                this.Container.Database.Id,
                                this.Container.Id,
                                this.ContainerResourceId,
                                error);
                        }
                    }
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
