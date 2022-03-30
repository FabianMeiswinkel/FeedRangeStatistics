# Feed Range Statistics Sample
This sample demonstrates how to retrieve statistics of logical partitions with most storage in the .Net SDK for Cosmos DB V3 as a replacement for the (`DocumentCollection.PartitinKeyRangeStatistics`)[https://docs.microsoft.com/dotnet/api/microsoft.azure.documents.documentcollection.partitionkeyrangestatistics] API in the V2 SDK.

**IMPORTANT**

Even in the V2 SDK reading collection metadata should not be done on the hot-path. Metadata operations are counting against a separate RU-budget (for the master partition) - so, even when you scale-up the RU/s for a container the number of metadata operations you can do is strictly limited. This is the reason why this sample exposes a FeedRangeStatisticsCache - you can configure a refresh interval and then on the hot-path you would simply check whether a logical partition has excessive storage by calling the `IFeedRangeStatisticsCache.TryGetStatistics` API. The statistics returned will only contain logical partitions exceeding a certain size - and it will be limited to the "worst offenders" - so, only the largest partitions are included in the sample. This behavior is identical as in the V2 SDK.



**Sample json payload of a container with a few sampled partitions** - the `/statistics` node contains the relevant information

```json
{
    "id": "SomeContainer",
    "indexingPolicy": {
        "indexingMode": "consistent",
        "automatic": true,
        "includedPaths": [
            {
                "path": "/*"
            }
        ],
        "excludedPaths": [
            {
                "path": "/\"_etag\"/?"
            }
        ]
    },
    "partitionKey": {
        "paths": [
            "/pk"
        ],
        "kind": "Hash"
    },
    "uniqueKeyPolicy": {
        "uniqueKeys": []
    },
    "conflictResolutionPolicy": {
        "mode": "LastWriterWins",
        "conflictResolutionPath": "/_ts",
        "conflictResolutionProcedure": ""
    },
    "geospatialConfig": {
        "type": "Geography"
    },
    "_rid": "IXQkALkAATQ=",
    "_ts": 1648601959,
    "_self": "dbs/IXQkAA==/colls/IXQkALkAATQ=/",
    "_etag": "\"0000b42a-0000-0d00-0000-6243ab670000\"",
    "_docs": "docs/",
    "_sprocs": "sprocs/",
    "_triggers": "triggers/",
    "_udfs": "udfs/",
    "_conflicts": "conflicts/",
    "statistics": [
        {
            "id": "4",
            "sizeInKB": 223616,
            "documentCount": 1497,
            "sampledDistinctPartitionKeyCount": 5,
            "partitionKeys": [
                {
                    "partitionKey": [
                        "ef579102-2595-4231-93bb-1f5eb1388c84"
                    ],
                    "sizeInKB": 84974
                },
                {
                    "partitionKey": [
                        "0431be46-bb48-42e9-8d4c-11ec183a33fc"
                    ],
                    "sizeInKB": 109571
                },
                {
                    "partitionKey": [
                        "7e60c216-87f3-4d74-9ef3-7ed75bf701dc"
                    ],
                    "sizeInKB": 4472
                },
                {
                    "partitionKey": [
                        "54d63a99-1765-44ba-9ec4-430da3968bd1"
                    ],
                    "sizeInKB": 6708
                },
                {
                    "partitionKey": [
                        "eebf2c07-5584-43ac-8353-48048f9b153d"
                    ],
                    "sizeInKB": 6708
                }
            ]
        },
        {
            "id": "6",
            "sizeInKB": 570630,
            "documentCount": 3724,
            "sampledDistinctPartitionKeyCount": 5,
            "partitionKeys": [
                {
                    "partitionKey": [
                        "00889b30-7a0b-45c1-832e-c083f1c747d1"
                    ],
                    "sizeInKB": 108419
                },
                {
                    "partitionKey": [
                        "b0c8d935-37f1-40bb-a7fa-140b7bbf11f8"
                    ],
                    "sizeInKB": 102713
                },
                {
                    "partitionKey": [
                        "301df60c-556f-4025-8bbf-0009e9ea94d2"
                    ],
                    "sizeInKB": 108419
                },
                {
                    "partitionKey": [
                        "0c27c6fd-5486-43c1-a415-cacf52c4b627"
                    ],
                    "sizeInKB": 108419
                },
                {
                    "partitionKey": [
                        "7d7ab073-5701-4e79-aa6d-06627c7248fe"
                    ],
                    "sizeInKB": 114126
                }
            ]
        },
        {
            "id": "3",
            "sizeInKB": 237024,
            "documentCount": 1502,
            "sampledDistinctPartitionKeyCount": 2,
            "partitionKeys": [
                {
                    "partitionKey": [
                        "82cf1c6d-9695-4c96-b1e4-b3c996848ea3"
                    ],
                    "sizeInKB": 116141
                },
                {
                    "partitionKey": [
                        "2d75d6ea-68e9-4876-b991-6f36860e8166"
                    ],
                    "sizeInKB": 116141
                }
            ]
        },
        {
            "id": "5",
            "sizeInKB": 120928,
            "documentCount": 738,
            "sampledDistinctPartitionKeyCount": 2,
            "partitionKeys": [
                {
                    "partitionKey": [
                        "e364252b-da4e-47f1-960e-d81db017f44f"
                    ],
                    "sizeInKB": 2418
                },
                {
                    "partitionKey": [
                        "e6013644-a2b3-4128-816e-55eb7e55dcfa"
                    ],
                    "sizeInKB": 116090
                }
            ]
        }
    ]
}
```



