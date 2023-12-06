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
               new List<string>() { "Data governance score", "Metadata completeness", "Ownership",
                    "Cataloging", "Classification", "Use",
                    "Data consumption purpose", "Access entitlement", "Quality",
                    "Data quality", "Authoritative data source" });

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

        if (!healthControlEntities.ContainsKey("Data governance score"))
        {
            var dataGovernanceEntity = await CreateHealthControlEntity(account, true, "Data governance score", Guid.Empty);
            healthControlEntities.Add("Data governance score", dataGovernanceEntity);
        }

        if (!healthControlEntities.ContainsKey("Metadata completeness"))
        {
            var metadataCompletenessEntity = await CreateHealthControlEntity(account, true, "Metadata completeness", healthControlEntities["Data governance score"].ObjectId);
            healthControlEntities.Add("Metadata completeness", metadataCompletenessEntity);
        }

        if (!healthControlEntities.ContainsKey("Ownership"))
        {
            var ownershipEntity = await CreateHealthControlEntity(account, false, "Ownership", healthControlEntities["Metadata completeness"].ObjectId);
            healthControlEntities.Add("Ownership", ownershipEntity);
        }

        if (!healthControlEntities.ContainsKey("Cataloging"))
        {
            var catalogingEntity = await CreateHealthControlEntity(account, false, "Cataloging", healthControlEntities["Metadata completeness"].ObjectId);
            healthControlEntities.Add("Cataloging", catalogingEntity);
        }

        if (!healthControlEntities.ContainsKey("Classification"))
        {
            var classificationEntity = await CreateHealthControlEntity(account, false, "Classification", healthControlEntities["Metadata completeness"].ObjectId);
            healthControlEntities.Add("Classification", classificationEntity);
        }

        if (!healthControlEntities.ContainsKey("Use"))
        {
            var useEntity = await CreateHealthControlEntity(account, true, "Use", healthControlEntities["Data governance score"].ObjectId);
            healthControlEntities.Add("Use", useEntity);
        }

        if (!healthControlEntities.ContainsKey("Data consumption purpose"))
        {
            var dataConsumptionPurposeEntity = await CreateHealthControlEntity(account, false, "Data consumption purpose", healthControlEntities["Use"].ObjectId);
            healthControlEntities.Add("Data consumption purpose", dataConsumptionPurposeEntity);
        }

        if (!healthControlEntities.ContainsKey("Access entitlement"))
        {
            var accessEntitlementEntity = await CreateHealthControlEntity(account, false, "Access entitlement", healthControlEntities["Use"].ObjectId);
            healthControlEntities.Add("Access entitlement", accessEntitlementEntity);
        }

        if (!healthControlEntities.ContainsKey("Quality"))
        {
            var qualityEntity = await CreateHealthControlEntity(account, true, "Quality", healthControlEntities["Data governance score"].ObjectId);
            healthControlEntities.Add("Quality", qualityEntity);
        }

        if (!healthControlEntities.ContainsKey("Data quality"))
        {
            var dataQualityEntity = await CreateHealthControlEntity(account, false, "Data quality", healthControlEntities["Quality"].ObjectId);
            healthControlEntities.Add("Data quality", dataQualityEntity);
        }

        if (!healthControlEntities.ContainsKey("Authoritative data source"))
        {
            var authoritativeDataSourceEntity = await CreateHealthControlEntity(account, false, "Authoritative data source", healthControlEntities["Quality"].ObjectId);
            healthControlEntities.Add("Authoritative data source", authoritativeDataSourceEntity);
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
            ScoreUnit = "Percentage",
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
