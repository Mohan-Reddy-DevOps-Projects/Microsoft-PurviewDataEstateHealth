namespace UnitTests.CosmosDBRawSQL;

using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.CosmosDBTestingMetadata;

[TestClass]
public class CosmosDBRawSQLTests
{
    private readonly Faker faker = new();
    private readonly Faker<TestEntityWrapper> testEntityFaker = new Faker<TestEntityWrapper>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.Name, f => f.Lorem.Word())
        .RuleFor(o => o.Description, f => f.Lorem.Sentence());

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task RawSQL_SelectCount_ReturnsExpectedCount()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(RawSQL_SelectCount_ReturnsExpectedCount)}"" is inconclusive.");
        }

        await CosmosDBClient.ResetContainer();

        // Arrange
        var testEntities = this.testEntityFaker.Generate(5);
        var tenantId = this.faker.Random.Guid().ToString();
        var (SucceededItems, _, _) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(testEntities, tenantId);
        Assert.AreEqual(testEntities.Count, SucceededItems.Count, "The number of items added to the database does not match the expected count.");

        // Act
        string sqlQueryText = "SELECT VALUE COUNT(1) FROM c";

        // Create a query definition
        var queryDefinition = new QueryDefinition(sqlQueryText);

        // Running the query and getting the iterator
        using (var queryResultSetIterator = CosmosDBClient.DBContainer!.GetItemQueryIterator<int>(queryDefinition))
        {
            if (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count > 0)
                {
                    var n = currentResultSet.First();
                    Assert.AreEqual(SucceededItems.Count, n, "The number of items in the database does not match the expected count.");
                }
            }
        }
    }
}