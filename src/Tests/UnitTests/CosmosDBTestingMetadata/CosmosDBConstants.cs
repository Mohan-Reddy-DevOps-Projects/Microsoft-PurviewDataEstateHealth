namespace UnitTests.CosmosDBTestingMetadata;

internal class CosmosDBConstants
{
    public const string DatabaseName = "TestDatabase";
    public const string ContainerName = "TestContainer";
    public const string CosmosDbEndpoint = "https://localhost:8081";
    // this is a well-known key for Azure Cosmos Emulator
    public const string CosmosDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    public const string PartitionKey = "/TenantId";
}
