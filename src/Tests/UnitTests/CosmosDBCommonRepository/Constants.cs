namespace UnitTests.CosmosDBCommonRepository;

using Bogus;

internal class Constants
{
    public const string DatabaseName = "TestDatabase";
    public const string ContainerName = "TestContainer";
    public const string CosmosDbEndpoint = "https://localhost:8081";
    // this is a well-known key for Azure Cosmos Emulator
    public const string CosmosDbKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    public static List<string> AvailableTenantIds = new Faker<string>()
                        .CustomInstantiator(f => Guid.NewGuid().ToString())
                        .Generate(new Faker().Random.Number(3, 6));
}
