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

internal sealed class HealthPBIReportComponent : IHealthPBIReportComponent
{
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;
    private readonly ReportProvider reportCommand;
    private readonly DatasetProvider datasetCommand;

    public HealthPBIReportComponent(IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration, ReportProvider reportCommand, DatasetProvider datasetCommand)
    {
        this.serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
        this.reportCommand = reportCommand;
        this.datasetCommand = datasetCommand;
    }

    public async Task CreateDataGovernanceReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false)
    {
        IDatasetRequest reportRequest = this.GetDataGovernanceReportRequest(profileId, workspaceId, powerBICredential);
        IDatasetRequest sharedDatasetRequest = this.GetDataGovernanceDatasetRequest(account, profileId, workspaceId, powerBICredential);
        Dataset sharedDataset = await this.CreateDataset(profileId, workspaceId, sharedDatasetRequest, cancellationToken);
        await this.CreateReport(sharedDataset, reportRequest, cancellationToken);
    }

    public async Task CreateDataQualityReport(AccountServiceModel account, Guid profileId, Guid workspaceId, PowerBICredential powerBICredential, CancellationToken cancellationToken, bool update = false)
    {
        IDatasetRequest reportRequest = this.GetDataQualityReportRequest(profileId, workspaceId, powerBICredential);
        IDatasetRequest sharedDatasetRequest = this.GetDataQualityDatasetRequest(account, profileId, workspaceId, powerBICredential);
        Dataset sharedDataset = await this.CreateDataset(profileId, workspaceId, sharedDatasetRequest, cancellationToken);
        await this.CreateReport(sharedDataset, reportRequest, cancellationToken);
    }

    public async Task<Dataset> CreateDataset(Guid profileId, Guid workspaceId, IDatasetRequest sharedDatasetRequest, CancellationToken cancellationToken, bool update = false)
    {
        Datasets datasets = await this.datasetCommand.List(sharedDatasetRequest, cancellationToken);
        Dataset sharedDataset = datasets.Value.FirstOrDefault(d => d.Name == sharedDatasetRequest.DatasetName);
        List<Dataset> allDatasettype = datasets.Value.Where(item => item.Name == sharedDatasetRequest.DatasetName).ToList();

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

    public async Task<Report> CreateReport(Dataset sharedDataset, IDatasetRequest reportRequest, CancellationToken cancellationToken, bool upgrade = false)
    {
        Reports existingReports = await this.reportCommand.List(reportRequest.ProfileId, reportRequest.WorkspaceId, cancellationToken);
        List<Report> allDatasettype = existingReports.Value.Where(item => item.Name == reportRequest.DatasetName).ToList();
        if (allDatasettype != null)
        {
            if (allDatasettype.Count != 1)
            {
                upgrade = true;
                foreach (var item in allDatasettype)
                {
                    await this.reportCommand.Delete(reportRequest.ProfileId, reportRequest.WorkspaceId, item.Id, cancellationToken);
                }
            }
        }

        if (upgrade || !existingReports.Value.Any(r => r.Name == reportRequest.DatasetName))
        {
            return await this.reportCommand.Bind(sharedDataset, reportRequest, cancellationToken);
        }

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
