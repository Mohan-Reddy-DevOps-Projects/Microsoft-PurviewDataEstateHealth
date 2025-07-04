namespace UnitTests.CosmosDBCommonRepository;

using Bogus;
using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using UnitTests.CosmosDBTestingMetadata;

[TestClass]
public class CommonRepositoryTests
{
    private readonly Faker faker = new();
    private readonly Faker<TestEntityWrapper> testEntityFaker = new Faker<TestEntityWrapper>()
        .RuleFor(o => o.Id, f => f.Random.Guid().ToString())
        .RuleFor(o => o.Name, f => f.Lorem.Word())
        .RuleFor(o => o.Description, f => f.Lorem.Sentence());

    [TestMethod]
    [Ignore("Entity without account id will no longer be supported!")]
    [Owner(Owners.CosmosDB)]
    [Obsolete("This test is obsolete and will be removed in the future.")]
    public async Task ICanAddEntityToContainerWithoutAccountId()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanAddEntityToContainerWithoutAccountId)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var addedEntity = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);
        Assert.AreEqual(accountIdentifier.TenantId, addedEntity.TenantId, "The TenantId of the added entity should be the same as the tenantId passed to the AddAsync method.");
        Assert.IsNull(addedEntity.AccountId, "The AccountId of the added entity should be null.");
        Assert.IsTrue(JObject.DeepEquals(entity.JObject, addedEntity.JObject), "The JObject of the added entity should be the same as the JObject of the entity passed to the AddAsync method.");

        var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(addedEntity.Id, accountIdentifier);
        Assert.AreEqual(addedEntity.Id, retrievedEntity!.Id, "The Id of the retrieved entity should be the same as the Id of the added entity.");
        Assert.AreEqual(addedEntity.TenantId, retrievedEntity.TenantId, "The TenantId of the retrieved entity should be the same as the TenantId of the added entity.");
        Assert.AreEqual(addedEntity.AccountId, retrievedEntity.AccountId, "The AccountId of the retrieved entity should be the same as the AccountId of the added entity.");
        Assert.IsTrue(JObject.DeepEquals(addedEntity.JObject, retrievedEntity.JObject), "The JObject of the retrieved entity should be the same as the JObject of the added entity.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanAddEntityToContainerWithAccountId()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanAddEntityToContainerWithAccountId)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var addedEntity = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);
        Assert.AreEqual(accountIdentifier.TenantId, addedEntity.TenantId, "The TenantId of the added entity should be the same as the tenantId passed to the AddAsync method.");
        Assert.AreEqual(accountIdentifier.AccountId, addedEntity.AccountId, "The AccountId of the added entity should be the same as the accountId passed to the AddAsync method.");
        Assert.IsTrue(JObject.DeepEquals(entity.JObject, addedEntity.JObject), "The JObject of the added entity should be the same as the JObject of the entity passed to the AddAsync method.");

        var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(addedEntity.Id, accountIdentifier);
        Assert.AreEqual(addedEntity.Id, retrievedEntity!.Id, "The Id of the retrieved entity should be the same as the Id of the added entity.");
        Assert.AreEqual(addedEntity.TenantId, retrievedEntity.TenantId, "The TenantId of the retrieved entity should be the same as the TenantId of the added entity.");
        Assert.AreEqual(addedEntity.AccountId, retrievedEntity.AccountId, "The AccountId of the retrieved entity should be the same as the AccountId of the added entity.");
        Assert.IsTrue(JObject.DeepEquals(addedEntity.JObject, retrievedEntity.JObject), "The JObject of the retrieved entity should be the same as the JObject of the added entity.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanAddMultipleEntitiesToContainer()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanAddMultipleEntitiesToContainer)}"" is inconclusive.");
        }

        var entities = this.testEntityFaker.Generate(10);
        foreach (var entity in entities)
        {
            Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(0, FailedItems.Count, "The FailedItems list should be empty.");
        Assert.AreEqual(entities.Count, SucceededItems.Count, "The number of added entities should be the same as the number of entities passed to the AddAsync method.");
        Assert.AreEqual(0, IgnoredItems.Count, "The IgnoredItems list should be empty.");

        var orderedList1 = entities.OrderBy(x => x.Id).ToList();
        var orderedList2 = SucceededItems.OrderBy(x => x.Id).ToList();

        for (var i = 0; i < orderedList1.Count; i += 1)
        {
            Assert.AreEqual(accountIdentifier.TenantId, orderedList2[i].TenantId, "The TenantId of the added entity should be the same as the tenantId passed to the AddAsync method.");
            Assert.AreEqual(accountIdentifier.AccountId, orderedList2[i].AccountId, "The AccountId of the added entity should be the same as the accountId passed to the AddAsync method.");
            Assert.IsTrue(JObject.DeepEquals(orderedList1[i].JObject, orderedList2[i].JObject), "The JObject of the added entity should be the same as the JObject of the entity passed to the AddAsync method.");

            var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(orderedList2[i].Id, accountIdentifier);
            Assert.AreEqual(orderedList2[i].Id, retrievedEntity!.Id, "The Id of the retrieved entity should be the same as the Id of the added entity.");
            Assert.AreEqual(orderedList2[i].TenantId, retrievedEntity.TenantId, "The TenantId of the retrieved entity should be the same as the TenantId of the added entity.");
            Assert.AreEqual(orderedList2[i].AccountId, retrievedEntity.AccountId, "The AccountId of the retrieved entity should be the same as the AccountId of the added entity.");
            Assert.IsTrue(JObject.DeepEquals(orderedList2[i].JObject, retrievedEntity.JObject), "The JObject of the retrieved entity should be the same as the JObject of the added entity.");
        }
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanAddManyEntitiesToContainer()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanAddManyEntitiesToContainer)}"" is inconclusive.");
        }

        var entities = this.testEntityFaker.Generate(1000);
        foreach (var entity in entities)
        {
            Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(entities.Count, SucceededItems.Count + FailedItems.Count, "The sum of succeeded entities and failed entities should be the same as the number of entities passed to the AddAsync method.");
        Assert.AreEqual(0, IgnoredItems.Count, "The IgnoredItems list should be empty.");

        foreach (var item in SucceededItems)
        {
            Assert.AreEqual(accountIdentifier.TenantId, item.TenantId, "The TenantId of the added entity should be the same as the tenantId passed to the AddAsync method.");
            Assert.AreEqual(accountIdentifier.AccountId, item.AccountId, "The AccountId of the added entity should be the same as the accountId passed to the AddAsync method.");

            var originalEntity = entities.Single(x => x.Id == item.Id);
            Assert.IsNotNull(originalEntity);

            Assert.IsTrue(JObject.DeepEquals(originalEntity.JObject, item.JObject), "The JObject of the added entity should be the same as the JObject of the entity passed to the AddAsync method.");

            var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(item.Id, accountIdentifier);
            Assert.AreEqual(item.Id, retrievedEntity!.Id, "The Id of the retrieved entity should be the same as the Id of the added entity.");
            Assert.AreEqual(item.TenantId, retrievedEntity.TenantId, "The TenantId of the retrieved entity should be the same as the TenantId of the added entity.");
            Assert.AreEqual(item.AccountId, retrievedEntity.AccountId, "The AccountId of the retrieved entity should be the same as the AccountId of the added entity.");
            Assert.IsTrue(JObject.DeepEquals(item.JObject, retrievedEntity.JObject), "The JObject of the retrieved entity should be the same as the JObject of the added entity.");
        }
    }

    [TestMethod]
    [Ignore("Entity without account id will no longer be supported!")]
    [Owner(Owners.CosmosDB)]
    [Obsolete("This test is obsolete and will be removed in the future.")]
    public async Task ICanUpdateEntityInContainerWithoutAccountId()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanUpdateEntityInContainerWithoutAccountId)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);

        var draftEntity = new TestEntityWrapper((JObject)entity.JObject.DeepClone())
        {
            TenantId = accountIdentifier.TenantId,
            AccountId = accountIdentifier.AccountId,
            Description = this.faker.Lorem.Sentence()
        };

        Assert.AreNotEqual(entity.Description, draftEntity.Description, "The description of the draft entity should be different from the description of the original entity.");
        Assert.IsTrue(AreJObjectsMostlyEqual(entity.JObject, draftEntity.JObject, "description"), "The JObject of the draft entity should be mostly the same as the JObject of the original entity, except for the Description property.");

        var updatedEntity = await CosmosDBClient.DBTestEntityRepository!.UpdateAsync(draftEntity, accountIdentifier);
        Assert.AreEqual(accountIdentifier.TenantId, updatedEntity.TenantId, "The TenantId of the updated entity should be the same as the tenantId passed to the UpdateAsync method.");
        Assert.IsNull(updatedEntity.AccountId, "The AccountId of the updated entity should be null.");
        Assert.AreEqual(draftEntity.Description, updatedEntity.Description, "The Description of the updated entity should be the same as the Description of the draft entity.");
        Assert.IsTrue(JObject.DeepEquals(draftEntity.JObject, updatedEntity.JObject), "The JObject of the updated entity should be the same as the JObject of the draft entity.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanUpdateMultipleEntitiesInContainer()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanUpdateMultipleEntitiesInContainer)}"" is inconclusive.");
        }

        var entities = this.testEntityFaker.Generate(10);
        foreach (var entity in entities)
        {
            Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(FailedItems.Count, 0, "The FailedItems list should be empty.");
        Assert.AreEqual(entities.Count, SucceededItems.Count, "The number of added entities should be the same as the number of entities passed to the AddAsync method.");
        Assert.AreEqual(0, IgnoredItems.Count, "The IgnoredItems list should be empty.");

        var draftEntities = SucceededItems.Select(item => new TestEntityWrapper((JObject)item.JObject.DeepClone())
        {
            TenantId = accountIdentifier.TenantId,
            AccountId = accountIdentifier.AccountId,
            Description = this.faker.Lorem.Sentence()
        }).ToList();

        foreach (var draftEntity in draftEntities)
        {
            var addedEntity = SucceededItems.Single(x => x.Id == draftEntity.Id);
            Assert.AreEqual(accountIdentifier.TenantId, addedEntity.TenantId, "The TenantId of the added entity should be the same as the tenantId passed to the AddAsync method.");
            Assert.AreEqual(accountIdentifier.AccountId, addedEntity.AccountId, "The AccountId of the added entity should be the same as the accountId passed to the AddAsync method.");
            Assert.AreNotEqual(addedEntity.Description, draftEntity.Description, "The description of the draft entity should be different from the description of the original entity.");
            Assert.IsTrue(AreJObjectsMostlyEqual(addedEntity.JObject, draftEntity.JObject, "description"), "The JObject of the draft entity should be mostly the same as the JObject of the original entity, except for the Description property.");
        }

        (SucceededItems, FailedItems) = await CosmosDBClient.DBTestEntityRepository!.UpdateAsync(draftEntities, accountIdentifier);
        Assert.AreEqual(FailedItems.Count, 0, "The FailedItems list should be empty.");
        Assert.AreEqual(draftEntities.Count, SucceededItems.Count, "The number of added entities should be the same as the number of entities passed to the UpdateAsync method.");

        foreach (var updatedEntity in SucceededItems)
        {
            var draftEntity = draftEntities.Single(x => x.Id == updatedEntity.Id);
            Assert.AreEqual(accountIdentifier.TenantId, updatedEntity.TenantId, "The TenantId of the updated entity should be the same as the tenantId passed to the UpdateAsync method.");
            Assert.AreEqual(accountIdentifier.AccountId, updatedEntity.AccountId, "The AccountId of the updated entity should be the same as the accountId passed to the UpdateAsync method.");
            Assert.AreEqual(draftEntity.Description, updatedEntity.Description, "The Description of the updated entity should be the same as the Description of the draft entity.");
            Assert.IsTrue(JObject.DeepEquals(draftEntity.JObject, updatedEntity.JObject), "The JObject of the updated entity should be the same as the JObject of the draft entity.");
        }
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanDeleteEntityFromContainer()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanDeleteEntityFromContainer)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);

        var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(entity.Id, accountIdentifier);
        Assert.IsNotNull(retrievedEntity, "The retrieved entity should not be null.");

        await CosmosDBClient.DBTestEntityRepository!.DeleteAsync(entity, accountIdentifier);

        retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(entity.Id, accountIdentifier);
        Assert.IsNull(retrievedEntity, "The retrieved entity should be null.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task ICanDeleteEntityByIdFromContainer()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(ICanDeleteEntityByIdFromContainer)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);

        var retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(entity.Id, accountIdentifier);
        Assert.IsNotNull(retrievedEntity, "The retrieved entity should not be null.");

        await CosmosDBClient.DBTestEntityRepository!.DeleteAsync(entity.Id, accountIdentifier);

        retrievedEntity = await CosmosDBClient.DBTestEntityRepository.GetByIdAsync(entity.Id, accountIdentifier);
        Assert.IsNull(retrievedEntity, "The retrieved entity should be null.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task GetByIdShouldReturnNullIfIdNotExist()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(GetByIdShouldReturnNullIfIdNotExist)}"" is inconclusive.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var retrievedEntity = await CosmosDBClient.DBTestEntityRepository!.GetByIdAsync(this.faker.Random.Guid().ToString(), accountIdentifier);
        Assert.IsNull(retrievedEntity, "The retrieved entity should be null.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task DeleteShouldNotThrowIfEntityNotExist()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(DeleteShouldNotThrowIfEntityNotExist)}"" is inconclusive.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        await CosmosDBClient.DBTestEntityRepository!.DeleteAsync(entity, accountIdentifier);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task DeleteShouldNotThrowIfEntityIdNotExist()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(DeleteShouldNotThrowIfEntityIdNotExist)}"" is inconclusive.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        await CosmosDBClient.DBTestEntityRepository!.DeleteAsync(this.faker.Random.Guid().ToString(), accountIdentifier);
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task AddManyEntitiesIgnoresExistingEntitiesWithoutError()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(AddManyEntitiesIgnoresExistingEntitiesWithoutError)}"" is inconclusive.");
        }

        var entities = this.testEntityFaker.Generate(10);
        foreach (var entity in entities)
        {
            Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };

        var (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(FailedItems.Count, 0, "The FailedItems list should be empty.");
        Assert.AreEqual(entities.Count, SucceededItems.Count, "The number of added entities should be the same as the number of entities passed to the AddAsync method.");
        Assert.AreEqual(0, IgnoredItems.Count, "The IgnoredItems list should be empty.");

        (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(FailedItems.Count, 0, "The FailedItems list should be empty.");
        Assert.AreEqual(0, SucceededItems.Count, "The number of added entities should be zero.");
        Assert.AreEqual(entities.Count, IgnoredItems.Count, "The number of ignored entities should be the same as the number of entities passed to the AddAsync method.");
    }

    [TestMethod]
    [Owner(Owners.CosmosDB)]
    public async Task GetAllAsyncShouldReturnAllEntities()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(GetAllAsyncShouldReturnAllEntities)}"" is inconclusive.");
        }

        await CosmosDBClient.ResetContainer();

        var entities = this.testEntityFaker.Generate(10);
        foreach (var entity in entities)
        {
            Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        }

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = this.faker.Random.Guid().ToString(),
            AccountId = this.faker.Random.Guid().ToString()
        };
        var (SucceededItems, FailedItems, IgnoredItems) = await CosmosDBClient.DBTestEntityRepository!.AddAsync(entities, accountIdentifier);

        Assert.AreEqual(FailedItems.Count, 0, "The FailedItems list should be empty.");
        Assert.AreEqual(entities.Count, SucceededItems.Count, "The number of added entities should be the same as the number of entities passed to the AddAsync method.");
        Assert.AreEqual(0, IgnoredItems.Count, "The IgnoredItems list should be empty.");

        var retrievedEntities = await CosmosDBClient.DBTestEntityRepository.GetAllAsync(accountIdentifier);
        Assert.AreEqual(entities.Count, retrievedEntities.Count(), "The number of retrieved entities should be the same as the number of entities passed to the AddAsync method.");

        foreach (var retrievedEntity in retrievedEntities)
        {
            var addedEntity = SucceededItems.Single(x => x.Id == retrievedEntity.Id);
            Assert.IsNotNull(addedEntity, "The added entity should not be null.");
            Assert.AreEqual(accountIdentifier.TenantId, retrievedEntity.TenantId, "The TenantId of the retrieved entity should be the same as the tenantId passed to the AddAsync method.");
            Assert.AreEqual(accountIdentifier.AccountId, retrievedEntity.AccountId, "The AccountId of the retrieved entity should be the same as the accountId passed to the AddAsync method.");
            Assert.IsTrue(JObject.DeepEquals(addedEntity.JObject, retrievedEntity.JObject), "The JObject of the retrieved entity should be the same as the JObject of the added entity.");
        }
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [Owner(Owners.CosmosDB)]
    public async Task AddEntityToContainerWithoutTenantIdWillThrowException()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(AddEntityToContainerWithoutTenantIdWillThrowException)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

#nullable disable
        var accountIdentifier = new AccountIdentifier
        {
            TenantId = null,
            AccountId = null
        };
        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);
#nullable enable
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [Owner(Owners.CosmosDB)]
    public async Task AddEntityToContainerWithInvalidTenantIdWillThrowException()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(AddEntityToContainerWithInvalidTenantIdWillThrowException)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = "Invalid GUID",
            AccountId = "Invalid GUID"
        };

        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [Owner(Owners.CosmosDB)]
    public async Task AddEntityToContainerWithAllZeroTenantIdWillThrowException()
    {
        if (!CosmosDBClient.TestingDBAvailable)
        {
            Assert.Inconclusive($@"The test case ""{nameof(AddEntityToContainerWithAllZeroTenantIdWillThrowException)}"" is inconclusive.");
        }

        var entity = this.testEntityFaker.Generate();
        Assert.IsNull(entity.TenantId, "The initial value of TenantId should be null.");
        Assert.IsNull(entity.AccountId, "The initial value of AccountId should be null.");

        var accountIdentifier = new AccountIdentifier
        {
            TenantId = Guid.Empty.ToString(),
            AccountId = Guid.Empty.ToString()
        };

        await CosmosDBClient.DBTestEntityRepository!.AddAsync(entity, accountIdentifier);
    }

    private static bool AreJObjectsMostlyEqual(JObject obj1, JObject obj2, string propertyNameToExclude)
    {
        // Clone obj1 to avoid modifying the original object
        var tempObj1 = (JObject)obj1.DeepClone();
        var tempObj2 = (JObject)obj2.DeepClone();

        // Remove or normalize the specific property in both objects
        tempObj1.Property(propertyNameToExclude)?.Remove();
        tempObj2.Property(propertyNameToExclude)?.Remove();

        // Compare the modified objects
        return JToken.DeepEquals(tempObj1, tempObj2);
    }
}