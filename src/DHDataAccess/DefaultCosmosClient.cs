namespace Microsoft.Purview.DataEstateHealth.DHDataAccess;

using Azure.Cosmos;

public interface IDefaultCosmosClient
{
    CosmosClient Client { get; }
}

public class DefaultCosmosClient(CosmosClient client) : IDefaultCosmosClient
{
    public CosmosClient Client { get; } = client;
}