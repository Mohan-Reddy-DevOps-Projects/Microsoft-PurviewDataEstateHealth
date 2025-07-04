namespace UnitTests.CosmosDBSerializer;

using Bogus;
using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Text;
using UnitTests.CosmosDBTestingMetadata;

[TestClass]
public class CosmosDBSerializerTests
{
    private static readonly CosmosWrapperSerializer serializer = new(new Mock<IDataEstateHealthRequestLogger>().Object);
    private readonly Faker<TestEntityWrapper> testEntityFaker = new Faker<TestEntityWrapper>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.Name, f => f.Lorem.Word())
        .RuleFor(o => o.Description, f => f.Lorem.Sentence());

    private static string ReadContentFromFile(string fileName)
    {
        // Get the current executing assembly location
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Get the directory of the current assembly
        var directoryPath = Path.GetDirectoryName(assemblyPath) ?? throw new InvalidOperationException("Get current working directory failed!");

        // This assumes the JSON files are located in a "TestData" folder within the output directory.
        var filePath = Path.Combine(directoryPath, "CosmosDBSerializer", "TestData", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Could not find file at path: {filePath}");
        }

        // Read and return the file content
        return File.ReadAllText(filePath);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonReaderException))]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ThrowsJsonReaderException_WhenInvalidDataProvided()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid JSON"));

        // Act
        _ = serializer.FromStream<object>(stream);

        // This point should not be reached; expect an exception
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectType_WhenValidStreamIsGiven()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("ControlNode1.json");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        // Act
        var result = serializer.FromStream<DHControlBaseWrapper>(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<DHControlNodeWrapper>(result);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectTenantId_WhenValidStreamIsGiven()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("ControlNode1.json");
        var jObject = JObject.Parse(jsonContent);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        // Act
        var result = serializer.FromStream<DHControlBaseWrapper>(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(jObject["TenantId"]?.ToObject<string>(), result.TenantId);
        Assert.AreEqual(jObject["AccountId"]?.ToObject<string>(), result.AccountId);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectType_WhenValidArrayStreamIsGiven()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("ControlArray1.json");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        // Act
        var result = serializer.FromStream<DHControlBaseWrapper[]>(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Length);
        Assert.IsInstanceOfType<DHControlNodeWrapper>(result[0]);
        Assert.IsInstanceOfType<DHControlNodeWrapper>(result[1]);
        Assert.IsInstanceOfType<DHControlNodeWrapper>(result[2]);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectTenantId_WhenValidArrayStreamIsGiven()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("ControlArray1.json");
        var jArray = JArray.Parse(jsonContent);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        // Act
        var result = serializer.FromStream<DHControlBaseWrapper[]>(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(jArray[0]["TenantId"]?.ToObject<string>(), result[0].TenantId);
        Assert.AreEqual(jArray[0]["AccountId"]?.ToObject<string>(), result[0].AccountId);
        Assert.AreEqual(jArray[1]["TenantId"]?.ToObject<string>(), result[1].TenantId);
        Assert.AreEqual(jArray[1]["AccountId"]?.ToObject<string>(), result[1].AccountId);
        Assert.AreEqual(jArray[2]["TenantId"]?.ToObject<string>(), result[2].TenantId);
        Assert.AreEqual(jArray[2]["AccountId"]?.ToObject<string>(), result[2].AccountId);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectType_WhenValidNonWrapperTypeStreamIsGiven()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("NonWrapper1.json");
        var jObject = JObject.Parse(jsonContent);
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        // Act
        var result = serializer.FromStream<NonWrapper1>(stream);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.PropertyA, jObject["propertyA"]?.ToObject<string>());
        Assert.AreEqual(result.PropertyB, jObject["propertyB"]?.ToObject<string>());
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void FromStream_ReturnsCorrectType_WhenIntArrayTypeStreamIsGiven()
    {
        // Arrange
        var numberSteam = new MemoryStream(Encoding.UTF8.GetBytes("[10,20]"));

        // Act
        var numberSteamResult = serializer.FromStream<int[]>(numberSteam);
        var numberSteamResultStr = string.Join(',', numberSteamResult);
        // Assert
        Assert.AreEqual(numberSteamResultStr, "10,20");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void ToSteam_ReturnsCorrectSteam()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("ControlNode1.json");
        var jObject1 = JObject.Parse(jsonContent);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        var result = serializer.FromStream<DHControlBaseWrapper>(memoryStream);

        var stream = serializer.ToStream(result);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using (StreamReader reader = new StreamReader(stream))
        {
            var content = reader.ReadToEnd();
            // Parse the string content to a JObject
            var jObject2 = JObject.Parse(content);

            Assert.AreEqual(JToken.DeepEquals(jObject1, jObject2), true);
        }
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void ToSteam_ReturnsCorrectSteamForNonWrapper()
    {
        // Arrange
        var jsonContent = ReadContentFromFile("NonWrapper1.json");
        var jObject1 = JObject.Parse(jsonContent);
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
        var result = serializer.FromStream<NonWrapper1>(memoryStream);

        var stream = serializer.ToStream(result);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using (StreamReader reader = new StreamReader(stream))
        {
            var content = reader.ReadToEnd();
            // Parse the string content to a JObject
            var jObject2 = JObject.Parse(content);

            Assert.AreEqual(JToken.DeepEquals(jObject1, jObject2), true);
        }
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public void Combo_NonWrapper()
    {
        var nonWrapper = new NonWrapper1
        {
            TenantId = Guid.NewGuid().ToString(),
            PropertyA = "A",
            PropertyB = "B"
        };

        var stream = serializer.ToStream(nonWrapper);

        var newNonWrapper = serializer.FromStream<NonWrapper1>(stream);

        Assert.AreEqual(nonWrapper.PropertyA, newNonWrapper.PropertyA);
        Assert.AreEqual(nonWrapper.PropertyB, newNonWrapper.PropertyB);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [Owner(Owners.CosmosDB)]
    public void ToStream_TenantIdShouldBePresent()
    {
        var testEntity = this.testEntityFaker.Generate();
        serializer.ToStream(testEntity);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    [Owner(Owners.CosmosDB)]
    public void ToStream_TenantIdShouldBeValidGUID()
    {
        var testEntity = this.testEntityFaker.Generate();
        testEntity.TenantId = "Invalid GUID";
        serializer.ToStream(testEntity);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [Owner(Owners.CosmosDB)]
    public void ToStream_TenantIdShouldNotBeEmptyGUID()
    {
        var testEntity = this.testEntityFaker.Generate();
        testEntity.TenantId = Guid.Empty.ToString();
        serializer.ToStream(testEntity);
    }
}

internal record NonWrapper1
{
    [JsonProperty(nameof(IContainerEntityWrapper.TenantId))]
    public required string TenantId { get; set; }

    [JsonProperty("propertyA")]
    public required string PropertyA { get; set; }

    [JsonProperty("propertyB")]
    public required string PropertyB { get; set; }
}