// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Extensions.Logging;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;

internal sealed class HealthDatasetUpgrade
{
    private readonly ILogger logger;
    private readonly DatasetProvider datasetCommand;
    private readonly IHealthPBIReportComponent healthPBIReportComponent;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;

    public HealthDatasetUpgrade(ILogger logger, DatasetProvider datasetCommand, IHealthPBIReportComponent healthPBIReportComponent, IPowerBICredentialComponent powerBICredentialComponent)
    {
        this.logger = logger;
        this.datasetCommand = datasetCommand;
        this.healthPBIReportComponent = healthPBIReportComponent;
        this.powerBICredentialComponent = powerBICredentialComponent;
    }

    /// <summary>
    /// Perform the dataset upgrade and return the dataset id mapping of the previous dataset id to the new dataset id.
    /// </summary>
    /// <param name="account"></param>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="schemaUpgradeSucceeded"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Dictionary<Guid, List<Dataset>>> UpgradeDatasets(AccountServiceModel account, Guid profileId, Guid workspaceId, bool schemaUpgradeSucceeded, CancellationToken cancellationToken)
    {
        IDatasetRequest datasetRequest = new DatasetRequest()
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
        };
        Datasets existingDatasets = await this.datasetCommand.List(datasetRequest, cancellationToken);
        IList<Dataset> datasetsToUpgrade = await this.GetUpgradableDatasets(profileId, workspaceId, existingDatasets, cancellationToken);
        this.logger.LogInformation($"Attempting to upgrade datasets. Datasets={JsonSerializer.Serialize(datasetsToUpgrade)}");
        IList<Dataset> upgradedDatasets = await this.Upgrade(account, profileId, workspaceId, schemaUpgradeSucceeded, datasetsToUpgrade, cancellationToken);
        Dictionary<Guid, List<Dataset>> datasetUpgrades = MapPreviousToNewDatasets(upgradedDatasets, existingDatasets.Value, schemaUpgradeSucceeded);
        this.logger.LogInformation($"Successfully upgraded datasets. Datasets={JsonSerializer.Serialize(datasetUpgrades)}");

        return datasetUpgrades;
    }

    /// <summary>
    /// When [shouldCreate] is [true] a new dataset will be created with the same name as each of the existing datasets. When [shouldCreate] is [false] the existing datasets are returned.
    /// </summary>
    /// <param name="account"></param>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="shouldCreate">Determines whether to create new datasets or not.</param>
    /// <param name="existingDatasets">The existing datasets.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task<IList<Dataset>> Upgrade(AccountServiceModel account, Guid profileId, Guid workspaceId, bool shouldCreate, IList<Dataset> existingDatasets, CancellationToken cancellationToken)
    {
        if (!shouldCreate)
        {
            return existingDatasets;
        }
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(Guid.Parse(account.Id), OwnerNames.Health, cancellationToken);

        IEnumerable<Task<Dataset>> createDatasetTasks = existingDatasets.Select(async dataset =>
        {
            IDatasetRequest sharedDatasetRequest = this.healthPBIReportComponent.GetSQLSharedDatasetRequest(account, profileId, workspaceId, powerBICredential, dataset.Name);
            return await this.healthPBIReportComponent.CreateDataset(profileId, workspaceId, sharedDatasetRequest, cancellationToken, true);
        });

        return await Task.WhenAll(createDatasetTasks);
    }

    /// <summary>
    /// Maps the newly created dataset to all of the existing datasets with the same name.
    /// When [shouldCreate] is [false] then the list of previous datasets will be empty. 
    /// </summary>
    /// <param name="upgradedDatasets">The newly created datasets</param>
    /// <param name="existingDatasets">The existing datasets</param>
    /// <param name="shouldCreate">Whether to create new datasets or not.</param>
    /// <returns></returns>
    public static Dictionary<Guid, List<Dataset>> MapPreviousToNewDatasets(IList<Dataset> upgradedDatasets, IList<Dataset> existingDatasets, bool shouldCreate)
    {
        ArgumentNullException.ThrowIfNull(upgradedDatasets);

        if (upgradedDatasets.Count == 0)
        {
            return existingDatasets.ToDictionary(x => Guid.Parse(x.Id), x => new List<Dataset>());
        }

        Dictionary<Guid, List<Dataset>> datasetUpgradeIds = new();
        if (!shouldCreate)
        {
            foreach (Dataset upgradedDataset in upgradedDatasets)
            {
                datasetUpgradeIds.TryAdd(Guid.Parse(upgradedDataset.Id), new List<Dataset>());
            }

            return datasetUpgradeIds;
        }

        ArgumentNullException.ThrowIfNull(existingDatasets, nameof(existingDatasets));
        ILookup<string, Dataset> existingDatasetsLookup = existingDatasets.ToLookup(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        foreach (Dataset upgradedDataset in upgradedDatasets)
        {
            IEnumerable<Dataset> matchingDatasets = existingDatasetsLookup[upgradedDataset.Name];
            datasetUpgradeIds.TryAdd(Guid.Parse(upgradedDataset.Id), matchingDatasets.ToList());
        }

        return datasetUpgradeIds;
    }

    /// <summary>
    /// Filter the existing datasets to only include the ones that need to be upgraded.
    /// Required: enumeration of existing datasets must be done in sequence to preserver ordering.
    /// </summary>
    /// <param name="profileId"></param>
    /// <param name="workspaceId"></param>
    /// <param name="existingDatasets"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<Dataset>> GetUpgradableDatasets(Guid profileId, Guid workspaceId, Datasets existingDatasets, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(existingDatasets);
        List<Dataset> upgradedDatasets = new();
        var datasetLength = Enum.GetValues(typeof(HealthDataset.Dataset)).Length;
        for (int i = 0; i < datasetLength; i++)
        {
            var enumValue = Enum.GetValues(typeof(HealthDataset.Dataset)).GetValue(i);
            string datasetName = SystemDatasets.Get()[enumValue.ToString()].Name;
            foreach (Dataset dataset in existingDatasets.Value.Where(x => x.Name.Equals(datasetName, StringComparison.Ordinal)))
            {
                DatasetRequest datasetRequest = new()
                {
                    ProfileId = profileId,
                    WorkspaceId = workspaceId,
                    DatasetContainer = "powerbi",
                    DatasetFileName = $"{dataset.Name}.pbix",
                };
                var properties = await this.datasetCommand.GetDatasetFileProperties(datasetRequest, cancellationToken);
                this.logger.LogInformation($"Upgrade check, name: {dataset.Name}, createdtime: {dataset.CreatedDate}, newversion time: {properties.LastModified}");
                if (properties != null && ShouldUpgradeDataset(dataset, properties.LastModified))
                {
                    upgradedDatasets.Add(dataset);
                }
            }
        }

        return upgradedDatasets
            .GroupBy(x => x.Name)
            .Select(g => g.OrderByDescending(dataset => dataset.CreatedDate).First())
            .ToList();
    }

    /// <summary>
    /// Determine if the dataset should be upgraded.
    /// </summary>
    /// <param name="dataset"></param>
    /// <param name="pbixUploadTime"></param>
    /// <returns></returns>
    private static bool ShouldUpgradeDataset(Dataset dataset, DateTimeOffset pbixUploadTime)
    {
        if (!dataset.CreatedDate.HasValue)
        {
            return false;
        }

        DateTime datasetCreatedUtc = dataset.CreatedDate.Value.ToUniversalTime();

        return datasetCreatedUtc <= pbixUploadTime;
    }

}
