namespace UnitTests.CosmosDBTestingMetadata;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

[TestClass]
internal class CosmosDBClient
{
    public static CosmosClient? DBClient { get; private set; }
    public static Database? DBDatabase { get; private set; }
    public static Container? DBContainer { get; private set; }
    public static TestEntityRepository? DBTestEntityRepository { get; private set; }
    public static bool TestingDBAvailable => DBTestEntityRepository != null;

    [AssemblyInitialize]
#pragma warning disable IDE0060 // Method has specific requirements for their signatures to be recognized by the MSTest framework.
    public static async Task TestSetup(TestContext context)
#pragma warning restore IDE0060
    {
        try
        {
            DBClient = new CosmosClient(CosmosDBConstants.CosmosDbEndpoint, CosmosDBConstants.CosmosDbKey, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                Serializer = new CosmosWrapperSerializer(new Mock<IDataEstateHealthRequestLogger>().Object),
                AllowBulkExecution = true
            });

            var databaseResponse = await DBClient.CreateDatabaseIfNotExistsAsync(CosmosDBConstants.DatabaseName);
            DBDatabase = databaseResponse.Database;
            var containerResponse = await DBDatabase.CreateContainerIfNotExistsAsync(CosmosDBConstants.ContainerName, CosmosDBConstants.PartitionKey);
            DBContainer = containerResponse.Container;

            DBTestEntityRepository = new TestEntityRepository(DBClient);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test setup failed: {ex.Message}");
        }
    }

    [AssemblyCleanup]
    public static async Task TestCleanup()
    {
        if (DBClient != null)
        {
            try
            {
                // Delete a container
                await DBClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.ContainerName).DeleteContainerAsync();

                // Delete a database
                await DBClient.GetDatabase(CosmosDBConstants.DatabaseName).DeleteAsync();

                DBClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test cleanup failed: {ex.Message}");
            }
        }
    }

    public static async Task ResetContainer()
    {
        if (DBClient != null)
        {
            try
            {
                // Delete a container
                await DBClient.GetContainer(CosmosDBConstants.DatabaseName, CosmosDBConstants.ContainerName).DeleteContainerAsync();

                // Create the container again
                await DBClient.GetDatabase(CosmosDBConstants.DatabaseName).CreateContainerIfNotExistsAsync(CosmosDBConstants.ContainerName, CosmosDBConstants.PartitionKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reset container failed: {ex.Message}");
            }
        }
    }
}
