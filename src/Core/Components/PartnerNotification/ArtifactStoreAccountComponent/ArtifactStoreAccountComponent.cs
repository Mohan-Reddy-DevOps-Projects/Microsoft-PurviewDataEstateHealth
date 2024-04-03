// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataAccess.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.ArtifactStoreClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        var healthControlKindFilter = new IndexedPropertyEqualityFilter(
           $"{nameof(HealthControlArtifactStoreEntity.HealthControlKind).UncapitalizeFirstChar()}",
           HealthControlKind.DataGovernance.ToString(),
           true);

        List<ArtifactStoreEntityDocument<HealthControlArtifactStoreEntity>> existingEntityList =
            (await this.artifactStoreAccessorService.ListResourcesByCategoryAsync<HealthControlArtifactStoreEntity>(
                accountId: Guid.Parse(account.Id),
                entityType: DataEstateHealthEntityTypes.DataEstateHealthControl.ToString(),
                filterText: new List<string>() { healthControlKindFilter.Predicate },
                parameters: new Dictionary<string, string>(healthControlKindFilter.Parameters),
                continuationToken: null,
                byPassObligations: true)).Items.ToList();

        this.dataEstateHealthRequestLogger.LogInformation($"List 11 OOTB controls returned {existingEntityList.Count} records.");

        foreach (var entity in existingEntityList)
        {
            if (!healthControlEntities.ContainsKey(entity.Properties.Name))
            {
                healthControlEntities.Add(entity.Properties.Name, entity.Properties);
            }
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataGovernanceScore.Name))
        {
            if (!healthControlEntities.ContainsKey("Data governance score"))
            {
                var dataGovernanceEntity = await this.CreateHealthControlEntity(account, true, OOTBControlTypes.DataGovernanceScore.Name, Guid.Empty);
                healthControlEntities.Add(OOTBControlTypes.DataGovernanceScore.Name, dataGovernanceEntity);
            }
            else
            {
                var entityToUpdate = healthControlEntities["Data governance score"];

                entityToUpdate.Name = OOTBControlTypes.DataGovernanceScore.Name;

                var indexedProperties = entityToUpdate.GetIndexedProperties();

                await this.artifactStoreAccessorService.CreateOrUpdateResourceAsync(
                            Guid.Parse(account.Id),
                            entityToUpdate.ObjectId.ToString(),
                            entityToUpdate.GetEntityType().ToString(),
                            entityToUpdate,
                            null,
                            indexedProperties);

                healthControlEntities.Remove("Data governance score");
                healthControlEntities.Add(OOTBControlTypes.DataGovernanceScore.Name, entityToUpdate);
            }
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.MetadataCompleteness.Name))
        {
            var metadataCompletenessEntity = await this.CreateHealthControlEntity(account, true, OOTBControlTypes.MetadataCompleteness.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.MetadataCompleteness.Name, metadataCompletenessEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Ownership.Name))
        {
            var ownershipEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.Ownership.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Ownership.Name, ownershipEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Cataloging.Name))
        {
            var catalogingEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.Cataloging.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Cataloging.Name, catalogingEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Classification.Name))
        {
            var classificationEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.Classification.Name, healthControlEntities[OOTBControlTypes.MetadataCompleteness.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Classification.Name, classificationEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Use.Name))
        {
            var useEntity = await this.CreateHealthControlEntity(account, true, OOTBControlTypes.Use.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Use.Name, useEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataConsumptionPurpose.Name))
        {
            var dataConsumptionPurposeEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.DataConsumptionPurpose.Name, healthControlEntities[OOTBControlTypes.Use.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.DataConsumptionPurpose.Name, dataConsumptionPurposeEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.AccessEntitlement.Name))
        {
            var accessEntitlementEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.AccessEntitlement.Name, healthControlEntities[OOTBControlTypes.Use.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.AccessEntitlement.Name, accessEntitlementEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.Quality.Name))
        {
            var qualityEntity = await this.CreateHealthControlEntity(account, true, OOTBControlTypes.Quality.Name, healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.Quality.Name, qualityEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.DataQuality.Name))
        {
            var dataQualityEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.DataQuality.Name, healthControlEntities[OOTBControlTypes.Quality.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.DataQuality.Name, dataQualityEntity);
        }

        if (!healthControlEntities.ContainsKey(OOTBControlTypes.AuthoritativeDataSource.Name))
        {
            var authoritativeDataSourceEntity = await this.CreateHealthControlEntity(account, false, OOTBControlTypes.AuthoritativeDataSource.Name, healthControlEntities[OOTBControlTypes.Quality.Name].ObjectId);
            healthControlEntities.Add(OOTBControlTypes.AuthoritativeDataSource.Name, authoritativeDataSourceEntity);
        }

        var dataGovernanceControlId = healthControlEntities[OOTBControlTypes.DataGovernanceScore.Name].ObjectId;

        //Update ownership.ParentControlId to data governance control id
        var ownershipUpdate = healthControlEntities[OOTBControlTypes.Ownership.Name];
        await this.UpdateParentControlIdOnMetadataCompletenessChildren(ownershipUpdate, account.Id, dataGovernanceControlId);

        //Update cataloging.ParentControlId to data governance control id
        var catalogingUpdate = healthControlEntities[OOTBControlTypes.Cataloging.Name];
        await this.UpdateParentControlIdOnMetadataCompletenessChildren(catalogingUpdate, account.Id, dataGovernanceControlId);

        //Update classification.ParentControlId to data governance control id
        var classificationUpdate = healthControlEntities[OOTBControlTypes.Classification.Name];
        await this.UpdateParentControlIdOnMetadataCompletenessChildren(classificationUpdate, account.Id, dataGovernanceControlId);

        this.dataEstateHealthRequestLogger.LogInformation("Insert OOTB controls ran successfully.");
    }

    private async Task UpdateParentControlIdOnMetadataCompletenessChildren(HealthControlArtifactStoreEntity healthControlArtifactStoreEntity, string accountId, Guid dataGovernanceControlId)
    {
        if (healthControlArtifactStoreEntity.ParentControlId != dataGovernanceControlId)
        {
            healthControlArtifactStoreEntity.ParentControlId = dataGovernanceControlId;

            var indexedProperties = healthControlArtifactStoreEntity.GetIndexedProperties();

            await this.artifactStoreAccessorService.CreateOrUpdateResourceAsync(
                        Guid.Parse(accountId),
                        healthControlArtifactStoreEntity.ObjectId.ToString(),
                        healthControlArtifactStoreEntity.GetEntityType().ToString(),
                        healthControlArtifactStoreEntity,
                        null,
                        indexedProperties);
        }
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
