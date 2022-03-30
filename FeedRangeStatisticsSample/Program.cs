using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FeedRangeStatisticsSample
{
    internal class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return MainAsync(args).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
            }
            catch (Exception error)
            {
                Console.WriteLine("ERROR: {0}", error);

                return 1;
            }
        }

        static async Task<int> MainAsync(string[] args)
        {
            const string SamplePKWithSampledStatistics = "7e60c216-87f3-4d74-9ef3-7ed75bf701dc";
            string connectionString = args[0];

            using CosmosClient client = new CosmosClientBuilder(connectionString).Build();
            Container container = client.GetContainer("SampleDatabase", "SomeContainer");

            ILogger<Program> logger = LoggerFactory
                .Create(logging => logging.AddConsole())
                .CreateLogger<Program>();

            var feedRangeStatisticsCache = 
                await container.CreateFeedRangeStatisticsCache(TimeSpan.FromSeconds(1), logger, CancellationToken.None);

            FeedRangeStatistics statistics = null;

            for (int i = 0; i < 300; i++)
            {
                logger.LogInformation(
                    "PK Statistics for '{0}': {1}",
                    "e364252b-da4e-47f1-960e-d81db017f44f",
                    feedRangeStatisticsCache.TryGetStatistics(
                        new PartitionKey("e364252b-da4e-47f1-960e-d81db017f44f"),
                        out statistics) ? statistics.SizeInKB.ToString() : "n/a");

                logger.LogInformation(
                    "PK Statistics for '{0}': {1}",
                    "b0c8d935-37f1-40bb-a7fa-140b7bbf11f8",
                    feedRangeStatisticsCache.TryGetStatistics(
                        new PartitionKey("b0c8d935-37f1-40bb-a7fa-140b7bbf11f8"),
                        out statistics) ? statistics.SizeInKB.ToString() : "n/a");

                logger.LogInformation(
                    "PK Statistics for '{0}': {1}",
                    "ef579102-2595-4231-93bb-1f5eb1388c84",
                    feedRangeStatisticsCache.TryGetStatistics(
                        new PartitionKey("ef579102-2595-4231-93bb-1f5eb1388c84"),
                        out statistics) ? statistics.SizeInKB.ToString() : "n/a");

                logger.LogInformation(
                    "PK Statistics for '{0}': {1}",
                    "e6013644-a2b3-4128-816e-55eb7e55dcfa",
                    feedRangeStatisticsCache.TryGetStatistics(
                        new PartitionKey("e6013644-a2b3-4128-816e-55eb7e55dcfa"),
                        out statistics) ? statistics.SizeInKB.ToString() : "n/a");

                logger.LogInformation(
                    "PK Statistics for '{0}': {1}",
                    SamplePKWithSampledStatistics,
                    feedRangeStatisticsCache.TryGetStatistics(
                        new PartitionKey(SamplePKWithSampledStatistics),
                        out statistics) ? statistics.SizeInKB.ToString() : "n/a");

                await Task.Delay(TimeSpan.FromSeconds(15));
            }

            return 0;
        }
    }
}