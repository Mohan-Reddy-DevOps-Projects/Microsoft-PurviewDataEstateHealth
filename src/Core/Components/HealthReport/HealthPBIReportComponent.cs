// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using System.Threading;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;

internal sealed class HealthPBIReportComponent : IHealthPBIReportComponent
{
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;
    private readonly ReportProvider reportCommand;
    private readonly DatasetProvider datasetCommand;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly Dictionary<string, string> datasetMapping = new Dictionary<string, string>
    {
        {  "Data_Governance_Dataset","Data governance"},
        { "DQ_Health_Dataset","DQ health report"}
    };


    public HealthPBIReportComponent(IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration, ReportProvider reportCommand, DatasetProvider datasetCommand, IDataEstateHealthRequestLogger logger)
    {
        this.serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
        this.reportCommand = reportCommand;
        this.datasetCommand = datasetCommand;
        this.logger = logger;
    }

    public async Task CreateDataGovernanceReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false, StartPBIRefreshMetadata metadata = null)
    {
        try
        {
            // Log the start of the report creation process with relevant details
            this.logger.LogInformation("Starting CreateDataGovernanceReport for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);

            IDatasetRequest reportRequest = this.GetDataGovernanceReportRequest(profileId, workspaceId, powerBICredential);
            IDatasetRequest sharedDatasetRequest = this.GetDataGovernanceDatasetRequest(account, profileId, workspaceId, powerBICredential);

            // Log dataset creation initiation
            this.logger.LogInformation("CreateDataGovernanceReport: Creating shared dataset for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);
            Dataset sharedDataset = await this.CreateDataset(profileId, workspaceId, sharedDatasetRequest, cancellationToken);

            if (metadata != null)
            {
                this.logger.LogInformation("CreateDataGovernanceReport: Metadata provided, updating DatasetUpgrades for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);

                List<Dataset> updatedDataSet = new List<Dataset>();
                updatedDataSet.Add(sharedDataset);
                if (metadata.DatasetUpgrades == null)
                {
                    metadata.DatasetUpgrades = new Dictionary<Guid, List<Dataset>>();
                }
                metadata.DatasetUpgrades.TryAdd(Guid.Parse(sharedDataset.Id), updatedDataSet);
            }

            // Log report creation initiation
            this.logger.LogInformation("CreateDataGovernanceReport: Creating report for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);
            await this.CreateReport(sharedDataset, reportRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log any unexpected errors with detailed context
            this.logger.LogError(ex, "An error occurred while creating the DataGovernanceReport for profileId: {ProfileId}, workspaceId: {WorkspaceId}. Exception: {ExceptionMessage}", profileId, workspaceId, ex.Message);
            throw; // rethrow exception
        }
    }

    public async Task CreateDataQualityReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false, StartPBIRefreshMetadata metadata = null)
    {
        try
        {
            this.logger.LogInformation("Starting CreateDataQualityReport for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);

            IDatasetRequest reportRequest = this.GetDataQualityReportRequest(profileId, workspaceId, powerBICredential);
            IDatasetRequest sharedDatasetRequest = this.GetDataQualityDatasetRequest(account, profileId, workspaceId, powerBICredential);
            //Note this classic report if it still remains in the workspace removing the dataset

            // Log dataset creation initiation
            this.logger.LogInformation("CreateDataQualityReport: Creating shared dataset for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);
            await this.DeleteOldDatasets(profileId, workspaceId, sharedDatasetRequest, "Data quality", cancellationToken);
            Dataset sharedDataset = await this.CreateDataset(profileId, workspaceId, sharedDatasetRequest, cancellationToken);
            if (metadata != null)
            {
                this.logger.LogInformation("CreateDataQualityReport: Metadata provided, updating DatasetUpgrades for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);

                List<Dataset> updatedDataSet = new List<Dataset>();
                updatedDataSet.Add(sharedDataset);
                if (metadata.DatasetUpgrades == null)
                {
                    metadata.DatasetUpgrades = new Dictionary<Guid, List<Dataset>>();
                }
                metadata.DatasetUpgrades.TryAdd(Guid.Parse(sharedDataset.Id), updatedDataSet);
            }
            //Note this classic/old report if it still remains in the workspace removing the report UI
            await this.DeleteOldReports(sharedDataset, reportRequest, "Data quality", cancellationToken);
            // Log report creation initiation
            this.logger.LogInformation("CreateDataQualityReport: Creating report for profileId: {ProfileId}, workspaceId: {WorkspaceId}", profileId, workspaceId);
            await this.CreateReport(sharedDataset, reportRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log any unexpected errors with detailed context
            this.logger.LogError(ex, "An error occurred while creating the CreateDataQualityReport for profileId: {ProfileId}, workspaceId: {WorkspaceId}. Exception: {ExceptionMessage}", profileId, workspaceId, ex.Message);
            throw; // rethrow exception
        }
    }

    public async Task<bool> DeleteOldDatasets(Guid profileId, Guid workspaceId, IDatasetRequest sharedDatasetRequest, string dataSetName, CancellationToken cancellationToken, bool update = false)
    {
        Datasets datasets = await this.datasetCommand.List(sharedDatasetRequest, cancellationToken);
        List<Dataset> allDatasettype = datasets.Value.Where(item => item.Name.ToLowerInvariant() == dataSetName.ToLowerInvariant()).ToList();

        if (allDatasettype != null)
        {

            foreach (var item in allDatasettype)
            {
                await this.datasetCommand.Delete(profileId, workspaceId, Guid.Parse(item.Id), cancellationToken);
            }
        }
        return true;
    }

    private static bool ShouldUpgradeDataset(Dataset dataset, DateTimeOffset pbixUploadTime)
    {
        if (!dataset.CreatedDate.HasValue)
        {
            return false;
        }

        DateTime datasetCreatedUtc = dataset.CreatedDate.Value.ToUniversalTime();

        return datasetCreatedUtc <= pbixUploadTime;
    }







    public async Task<Dataset> CreateDataset(Guid profileId, Guid workspaceId, IDatasetRequest sharedDatasetRequest, CancellationToken cancellationToken, bool update = false)
    {
        Datasets datasets = await this.datasetCommand.List(sharedDatasetRequest, cancellationToken);
        Dataset sharedDataset = datasets.Value.FirstOrDefault(d => d.Name == sharedDatasetRequest.DatasetName);
        List<Dataset> allDatasettype = datasets.Value.Where(item => item.Name.ToLowerInvariant() == sharedDatasetRequest.DatasetName.ToLowerInvariant()).ToList();

        //Remove dataset that automatically gets generated when a report ux is imported.
        List<Dataset> allDatasettypeDG = datasets.Value.Where(item => item.Name.ToLowerInvariant() == this.datasetMapping[sharedDatasetRequest.DatasetName].ToLowerInvariant()).ToList();
        if (allDatasettypeDG != null)
        {
            foreach (var item in allDatasettypeDG)
            {
                await this.datasetCommand.Delete(profileId, workspaceId, Guid.Parse(item.Id), cancellationToken);
            }
        }

        if (allDatasettype != null)
        {
            if (allDatasettype.Count != 1)
            {
                update = true;
                foreach (var item in allDatasettype)
                {
                    await this.datasetCommand.Delete(profileId, workspaceId, Guid.Parse(item.Id), cancellationToken);
                }
            }
            else
            {
                var properties = await this.datasetCommand.GetDatasetFileProperties(sharedDatasetRequest, cancellationToken);
                if (properties != null && ShouldUpgradeDataset(sharedDataset, properties.LastModified))
                {
                    update = true;
                    //Delete the existing Dataset and create a new one since a newer version is there in the storage
                    await this.datasetCommand.Delete(profileId, workspaceId, Guid.Parse(sharedDataset.Id), cancellationToken);
                }
            }
        }

        try
        {
            if (update)
            {
                sharedDataset = await this.datasetCommand.Create(sharedDatasetRequest, cancellationToken);
            }
            else
            {
                sharedDataset ??= await this.datasetCommand.Create(sharedDatasetRequest, cancellationToken);
            }
        }
        catch (ServiceException ex) when (ex.ServiceError.Category == ErrorCategory.ServiceError && ex.ServiceError.Code == ErrorCode.PowerBI_ReportDeleteFailed.Code)
        {
            IDatasetRequest deleteRequest = new DatasetRequest()
            {
                DatasetId = Guid.Parse(sharedDataset.Id),
                ProfileId = profileId,
                WorkspaceId = workspaceId
            };
            await this.datasetCommand.Delete(deleteRequest.ProfileId, deleteRequest.WorkspaceId, deleteRequest.DatasetId, cancellationToken);
            throw;
        }

        return sharedDataset;
    }

    public async Task<bool> DeleteOldReports(Dataset sharedDataset, IDatasetRequest reportRequest, string reportName, CancellationToken cancellationToken, bool upgrade = false)
    {
        Reports existingReports = await this.reportCommand.List(reportRequest.ProfileId, reportRequest.WorkspaceId, cancellationToken);
        List<Report> allDatasettype = existingReports.Value.Where(item => item.Name.ToLowerInvariant() == reportName.ToLowerInvariant()).ToList();
        if (allDatasettype != null)
        {
            foreach (var item in allDatasettype)
            {
                await this.reportCommand.Delete(reportRequest.ProfileId, reportRequest.WorkspaceId, item.Id, cancellationToken);
            }
        }

        return true;
    }

    public async Task<Report> CreateReport(Dataset sharedDataset, IDatasetRequest reportRequest, CancellationToken cancellationToken, bool upgrade = false)
    {
        this.logger.LogInformation("Listing existing reports for ProfileId: {ProfileId}, WorkspaceId: {WorkspaceId}", reportRequest.ProfileId, reportRequest.WorkspaceId);
        Reports existingReports = await this.reportCommand.List(reportRequest.ProfileId, reportRequest.WorkspaceId, cancellationToken);
        List<Report> allDatasettype = existingReports.Value.Where(item => item.Name.ToLowerInvariant() == reportRequest.DatasetName.ToLowerInvariant()).ToList();

        if (allDatasettype != null)
        {
            if (allDatasettype.Count != 1)
            {
                this.logger.LogInformation("Multiple reports found with the same DatasetName, considering upgrade for DatasetName: {DatasetName}", reportRequest.DatasetName);
                upgrade = true;
                foreach (var item in allDatasettype)
                {
                    this.logger.LogInformation("Deleting report with Id: {ReportId} for DatasetName: {DatasetName}", item.Id, reportRequest.DatasetName);
                    await this.reportCommand.Delete(reportRequest.ProfileId, reportRequest.WorkspaceId, item.Id, cancellationToken);
                }
            }
            else
            {
                foreach (var item in allDatasettype)
                {
                    this.logger.LogInformation("Deleting existing report with Id: {ReportId} for DatasetName: {DatasetName}, ProfileId: {ProfileId}, WorkspaceId: {WorkspaceId}", item.Id, reportRequest.DatasetName, reportRequest.ProfileId, reportRequest.WorkspaceId);
                    //Since there is not way to detect if the PBI ux has changed delete everytime so new report can be picked from storage
                    await this.reportCommand.Delete(reportRequest.ProfileId, reportRequest.WorkspaceId, item.Id, cancellationToken);
                }
            }

        }

        this.logger.LogInformation("Fetching the latest list of reports after deletion for ProfileId: {ProfileId}, WorkspaceId: {WorkspaceId}", reportRequest.ProfileId, reportRequest.WorkspaceId);
        existingReports = await this.reportCommand.List(reportRequest.ProfileId, reportRequest.WorkspaceId, cancellationToken);

        if (upgrade || !existingReports.Value.Any(r => r.Name == reportRequest.DatasetName))
        {
            this.logger.LogInformation("Creating or upgrading report for DatasetName: {DatasetName}", reportRequest.DatasetName);
            return await this.reportCommand.Bind(sharedDataset, reportRequest, cancellationToken);
        }

        this.logger.LogInformation("No upgrade required. Returning the last report with DatasetName: {DatasetName}", reportRequest.DatasetName);
        return existingReports.Value.Last();
    }

    public IDatasetRequest GetSQLSharedDatasetRequest(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, string sharedDatasetName)
    {
        return new DatasetRequest()
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            DatasetContainer = "powerbi",
            DatasetFileName = $"{sharedDatasetName}.pbix",
            DatasetName = sharedDatasetName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential,
            SkipReport = true,
            Parameters = new Dictionary<string, string>()
            {
                { SQLConstants.Server, this.serverlessPoolConfiguration.SqlEndpoint.Split(';').First() },
                { SQLConstants.Database, $"{OwnerNames.Health}_1" },
                { SQLConstants.DatabaseSchema, account.Id },
                { "TENANT_ID", account.TenantId }
            }
        };
    }

    public IDatasetRequest GetReportRequest(Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, string reportName)
    {
        return new DatasetRequest()
        {
            ProfileId = profileId,
            WorkspaceId = workspaceId,
            DatasetContainer = "powerbi",
            DatasetFileName = $"{reportName}.pbix",
            DatasetName = reportName,
            OptimizedDataset = true,
            PowerBICredential = powerBICredential
        };
    }

    private IDatasetRequest GetDataGovernanceReportRequest(Guid profileId, Guid workspaceId, PowerBICredential powerBICredential)
    {
        string reportName = HealthReportNames.DataGovernance;
        return this.GetReportRequest(profileId, workspaceId, powerBICredential, reportName);
    }

    private IDatasetRequest GetDataGovernanceDatasetRequest(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential)
    {
        IDataset sharedDataset = SystemDatasets.Get()[HealthDataset.Dataset.DataGovernance.ToString()];
        return this.GetSQLSharedDatasetRequest(account, profileId, workspaceId, powerBICredential, sharedDataset.Name);
    }

    private IDatasetRequest GetDataQualityReportRequest(Guid profileId, Guid workspaceId, PowerBICredential powerBICredential)
    {
        string reportName = HealthReportNames.DataQuality;
        return this.GetReportRequest(profileId, workspaceId, powerBICredential, reportName);
    }

    private IDatasetRequest GetDataQualityDatasetRequest(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential)
    {
        IDataset sharedDataset = SystemDatasets.Get()[HealthDataset.Dataset.DataQuality.ToString()];
        return this.GetSQLSharedDatasetRequest(account, profileId, workspaceId, powerBICredential, sharedDataset.Name);
    }
}
