// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataAccess.DataAccess;
using Microsoft.Purview.ArtifactStoreClient;
using Microsoft.Azure.Purview.DataAccess.DataAccess.Shared;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal sealed class ArtifactStoreAccountComponent : IArtifactStoreAccountComponent
{
    private IArtifactStoreAccessorService artifactStoreAccessorService;

    private IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    public ArtifactStoreAccountComponent(IArtifactStoreAccessorServiceBuilder artifactStoreAccessorServiceBuilder, IDataEstateHealthRequestLogger dataEstateHealthRequestLogger)
    {
        this.artifactStoreAccessorService = artifactStoreAccessorServiceBuilder.Build();
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
    }

    /// <inheritdoc/>
    public async Task CreateArtifactStoreResources(AccountServiceModel account, CancellationToken cancellationToken)
    {
        Dictionary<string, HealthControlArtifactStoreEntity> healthControlEntities = new Dictionary<string, HealthControlArtifactStoreEntity>();

        var nameFilters = new IndexedPropertyListEqualityFilter(
               $"{nameof(HealthControlArtifactStoreEntity.Name).UncapitalizeFirstChar()}",
               OOTBControlTypes.GetListOfNames());

        List<ArtifactStoreEntityDocument<HealthControlArtifactStoreEntity>> existingEntityList =
            (await this.artifactStoreAccessorService.ListResourcesByCategoryAsync<HealthControlArtifactStoreEntity>(
                accountId: Guid.Parse(account.Id),
                entityType: DataEstateHealthEntityTypes.DataEstateHealthControl.ToString(),
                filterText: new List<string>() { nameFilters.Predicate },
                parameters: new Dictionary<string, string>(nameFilters.Parameters),
                continuationToken: null,
                byPassObligations: true)).Items.ToList();

        this.dataEstateHealthRequestLogger.LogInformation($"List 11 OOTB controls returned {existingEntityList.Count} records.");

        foreach(var entity in existingEntityList)
        {
            healthControlEntities.Add(entity.Properties.Name, entity.Properties);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataGovernanceScore.Name))
        {
            var dataGovernanceEntity = await CreateHealthControlEntity(account, true, OOTBControlTypes.DataGovernanceScore.Name, Guid.Empty);
            healthControlEntities.Add(OOTBControlTypes.DataGovernanceScore.Name, dataGovernanceEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.MetadataCompleteness.Name))
        {
            var metadataCompletenessEntity = await CreateHealthControlEntity(account, true, OOTBControlTypes.MetadataCompleteness.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.MetadataCompleteness.Name, metadataCompletenessEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Ownership.Name))
        {
            var ownershipEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.Ownership.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Ownership.Name, ownershipEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Cataloging.Name))
        {
            var catalogingEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.Cataloging.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Cataloging.Name, catalogingEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Classification.Name))
        {
            var classificationEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.Classification.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Classification.Name, classificationEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Use.Name))
        {
            var useEntity = await CreateHealthControlEntity(account, true, OOTBControlTypes.Use.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Use.Name, useEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataConsumptionPurpose.Name))
        {
            var dataConsumptionPurposeEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.DataConsumptionPurpose.Name, healthControlEntities[OOTBControlTypes.Use.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.DataConsumptionPurpose.Name, dataConsumptionPurposeEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.AccessEntitlement.Name))
        {
            var accessEntitlementEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.AccessEntitlement.Name, healthControlEntities[OOTBControlTypes.Use.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.AccessEntitlement.Name, accessEntitlementEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Quality.Name))
        {
            var qualityEntity = await CreateHealthControlEntity(account, true, OOTBControlTypes.Quality.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Quality.Name, qualityEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataQuality.Name))
        {
            var dataQualityEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.DataQuality.Name, healthControlEntities[OOTBControlTypes.Quality.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.DataQuality.Name, dataQualityEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.AuthoritativeDataSource.Name))
        {
            var authoritativeDataSourceEntity = await CreateHealthControlEntity(account, false, OOTBControlTypes.AuthoritativeDataSource.Name, healthControlEntities[OOTBControlTypes.Quality.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.AuthoritativeDataSource.Name, authoritativeDataSourceEntity);
        }

        this.dataEstateHealthRequestLogger.LogInformation("Insert OOTB controls ran successfully.");
    }

    private async Task<HealthControlArtifactStoreEntity> CreateHealthControlEntity(AccountServiceModel account, bool isCompositeControl, string name, Guid parentControlId)
    {
        var entityToInsert = new HealthControlArtifactStoreEntity()
        {
            HealthControlKind = HealthControlKind.DataGovernance,
            ControlStatus = HealthResourceStatus.Active,
            ControlType = HealthResourceType.System,
            CreatedAt = DateTime.UtcNow,
            Description = "This overall control helps determine current data governance maturity posture based on the Cloud Data Management Council's framework.",
            EndsAt = DateTime.MaxValue,
            HealthStatus = "Active",
            IsCompositeControl = isCompositeControl,
            ModifiedAt = DateTime.UtcNow,
            Name = name,
            ObjectId = Guid.NewGuid(),
            OwnerContact = new OwnerContact()
            {
                ObjectId = Guid.Empty,
                DisplayName = "System",
            },
            ParentControlId = parentControlId,
            ScoreUnit = "%",
            StartsAt = DateTime.MinValue,
            TargetScore = 80,
            TrendUrl = null,
            Version = "",
        };

        var indexedProperties = entityToInsert.GetIndexedProperties(); 

        return (await this.artifactStoreAccessorService.CreateOrUpdateResourceAsync(
                    Guid.Parse(account.Id),
                    entityToInsert.ObjectId.ToString(),
                    entityToInsert.GetEntityType().ToString(),
                    entityToInsert,
                    null,
                    indexedProperties)).Properties;
    }
}
