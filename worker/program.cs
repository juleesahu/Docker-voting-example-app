using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

namespace Worker
{
    public class Program
    {
        private static readonly string TableName = "votes"; // DynamoDB table name
        private static readonly string Region = "eu-west-1"; // Specify your AWS region

        public static int Main(string[] args)
        {
            try
            {
                var dynamoDbClient = OpenDynamoDbClient();
                EnsureTableExists(dynamoDbClient, TableName);

                // Continuously monitor the SQS queue for incoming messages
                while (true)
                {
                    // Slow down to prevent CPU spikes, only query each 100ms
                    Thread.Sleep(100);

                    var message = GetSqsMessage(); // Replace with your SQS message fetching logic
                    if (message != null)
                    {
                        var vote = JsonConvert.DeserializeObject<Vote>(message);
                        Console.WriteLine($"Processing vote for '{vote.Vote}' by '{vote.VoterId}'");
                        
                        // Update the vote in DynamoDB
                        UpdateVote(dynamoDbClient, vote.VoterId, vote.Vote);
                        
                        // Delete the message from SQS after processing
                        DeleteSqsMessage(message); // Replace with your SQS message deletion logic
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
        }

        private static AmazonDynamoDBClient OpenDynamoDbClient()
        {
            // Create a new DynamoDB client
            return new AmazonDynamoDBClient(Amazon.RegionEndpoint.GetBySystemName(Region));
        }

        private static void EnsureTableExists(AmazonDynamoDBClient client, string tableName)
        {
            // Check if the table exists
            var tables = client.ListTablesAsync().Result;
            if (!tables.TableNames.Contains(tableName))
            {
                // Create the DynamoDB table
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("PK", KeyType.HASH),  // Partition key
                        new KeySchemaElement("SK", KeyType.RANGE)  // Sort key
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("PK", ScalarAttributeType.S),
                        new AttributeDefinition("SK", ScalarAttributeType.S)
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(5, 5) // Adjust capacity as needed
                };
                
                client.CreateTableAsync(request).Wait();
                Console.WriteLine("Created DynamoDB table: " + tableName);
            }
        }

        private static void UpdateVote(AmazonDynamoDBClient client, string voterId, string vote)
        {
            var table = Table.LoadTable(client, TableName);
            var voteKey = $"POLL#cats-vs-dogs"; // Replace with your poll identifier
            var userKey = $"USERID#{voterId}";

            // Use DynamoDB transactions to ensure atomic operations
            var transactWriteItems = new List<TransactWriteItem>
            {
                new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = TableName,
                        Item = new Dictionary<string, AttributeValue>
                        {
                            { "PK", new AttributeValue { S = voteKey } },
                            { "SK", new AttributeValue { S = userKey } },
                            { "vote", new AttributeValue { S = vote } }
                        },
                        ConditionExpression = "attribute_not_exists(PK)"
                    }
                },
                new TransactWriteItem
                {
                    Update = new Update
                    {
                        TableName = TableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "PK", new AttributeValue { S = voteKey } },
                            { "SK", new AttributeValue { S = $"VOTE#{vote}" } }
                        },
                        UpdateExpression = "ADD voteCount :increment",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            { ":increment", new AttributeValue { N = "1" } }
                        }
                    }
                }
            };

            // Execute the transaction
            client.TransactWriteItemsAsync(new TransactWriteItemsRequest { TransactItems = transactWriteItems }).Wait();
        }

        private static void DeleteSqsMessage(string message)
        {
            // Implement your SQS message deletion logic here
        }

        private static string GetSqsMessage()
        {
            // Implement your SQS message fetching logic here
            return null; // Placeholder return statement
        }

        // Define a class to represent a vote
        public class Vote
        {
            public string VoterId { get; set; }
            public string Vote { get; set; }
        }
    }
}
