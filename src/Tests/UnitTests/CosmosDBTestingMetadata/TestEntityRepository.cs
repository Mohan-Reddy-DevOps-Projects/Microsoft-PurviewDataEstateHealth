namespace UnitTests.CosmosDBTestingMetadata;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Moq;
using Newtonsoft.Json.Linq;

internal class TestEntityWrapper(JObject jObject) : ContainerEntityBaseWrapper<TestEntityWrapper>(jObject)
{
    public TestEntityWrapper() : this([]) { }

    [EntityProperty("name")]
    public string Name
    {
        get => this.GetPropertyValue<string>("name");
        set => this.SetPropertyValue("name", value);
    }

    [EntityProperty("description")]
    public string Description
    {
        get => this.GetPropertyValue<string>("description");
        set => this.SetPropertyValue("description", value);
    }
}

internal class TestEntityRepository(CosmosClient cosmosClient)
    : CommonRepository<TestEntityWrapper>(new Mock<IDataEstateHealthRequestLogger>().Object)
{
    protected override Container CosmosContainer => cosmosClient.GetDatabase(CosmosDBConstants.DatabaseName).GetContainer(CosmosDBConstants.ContainerName);
}