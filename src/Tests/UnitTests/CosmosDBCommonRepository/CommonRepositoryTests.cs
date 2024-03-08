namespace UnitTests.CosmosDBCommonRepository;

using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class CommonRepositoryTests
{
    private CosmosClient? cosmosClient;
    private TestEntityRepository? testEntityRepository;
    private readonly Faker<TestEntityWrapper> testEntityFaker = new Faker<TestEntityWrapper>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.Name, f => f.Lorem.Word())
        .RuleFor(o => o.Description, f => f.Lorem.Sentence())
        .RuleFor(o => o.TenantId, f => f.PickRandom(Constants.AvailableTenantIds));
    private bool skipTest = false;

    [TestInitialize]
    public async Task TestSetup()
    {
        try
        {
            this.cosmosClient = new CosmosClient(Constants.CosmosDbEndpoint, Constants.CosmosDbKey, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                Serializer = new CosmosWrapperSerializer(),
                AllowBulkExecution = true
            });

            var databaseResponse = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(Constants.DatabaseName);
            var database = databaseResponse.Database;
            await database.CreateContainerIfNotExistsAsync(Constants.ContainerName, "/TenantId");

            this.testEntityRepository = new TestEntityRepository(this.cosmosClient);
        }
        catch (Exception ex)
        {
            this.skipTest = true;
            Console.WriteLine($"Test setup failed: {ex.Message}");
        }
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        if (this.cosmosClient != null)
        {
            try
            {
                // Delete a container
                await this.cosmosClient.GetContainer(Constants.DatabaseName, Constants.ContainerName).DeleteContainerAsync();

                // Delete a database
                await this.cosmosClient.GetDatabase(Constants.DatabaseName).DeleteAsync();

                this.cosmosClient.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test cleanup failed: {ex.Message}");
            }
        }
    }

    [TestMethod]
    public async Task ICanAddEntityToContainer()
    {
        if (this.skipTest)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanAddEntityToContainer)}"" is inconclusive.");
        }
        var entity = this.testEntityFaker.Generate();
        var addedEntity = await this.testEntityRepository!.AddAsync(entity, entity.TenantId!);
        Assert.AreEqual(entity.Id, addedEntity.Id);
        var readEntity = await this.testEntityRepository!.GetByIdAsync(entity.Id, entity.TenantId!);
        Assert.AreEqual(entity.Id, readEntity!.Id);
    }
}