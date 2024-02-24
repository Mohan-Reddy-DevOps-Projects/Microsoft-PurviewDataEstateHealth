// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.Purview.DataGovernance.Reporting.Services;
using Microsoft.Rest;

internal class RefreshComponent : IRefreshComponent
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IPowerBIService powerBIService;
    private readonly CapacityProvider capacityAssignment;
    private readonly IHealthProfileCommand profileCommand;
    private static readonly IEnumerable<IDataset> allowedDatasets = SystemDatasets.Get().Values;

    public RefreshComponent(IDataEstateHealthRequestLogger logger, PowerBIProvider powerBIProvider, CapacityProvider capacityAssignment, IHealthProfileCommand profileCommand)
    {
        this.logger = logger;
        this.powerBIService = powerBIProvider.PowerBIService;
        this.capacityAssignment = capacityAssignment;
        this.profileCommand = profileCommand;
    }

    /// <inheritdoc/>
    public async Task<IList<RefreshDetailsModel>> GetRefreshStatus(IList<RefreshLookup> refreshLookups, CancellationToken cancellationToken)
    {
        IEnumerable<Task<RefreshDetailsModel>> refreshTasks = refreshLookups.Select(async refreshLookup =>
        {
            DatasetRefreshDetail refreshStatus = await this.GetRefreshStatus(refreshLookup, cancellationToken);
            if (refreshStatus == null)
            {
                return null;
            }

            return new RefreshDetailsModel()
            {
                ProfileId = refreshLookup.ProfileId,
                WorkspaceId = refreshLookup.WorkspaceId,
                DatasetId = refreshLookup.DatasetId,
                EndTime = refreshStatus.EndTime,
                StartTime = refreshStatus.StartTime,
                Status = refreshStatus.Status,
                Type = refreshStatus.Type,
                CurrentRefreshType = refreshStatus.CurrentRefreshType,
                NumberOfAttempts = refreshStatus.NumberOfAttempts,
                Messages = refreshStatus.Messages?.Select(x => new Microsoft.Purview.DataGovernance.Reporting.Models.EngineMessage()
                {
                    Code = x.Code,
                    Message = x.Message,
                    Type = x.Type,
                }).ToArray(),
            };
        });

        RefreshDetailsModel[] results = await Task.WhenAll(refreshTasks);

        return results.Where(x => x != null).ToList();
    }

    /// <inheritdoc/>
    public async Task<IList<RefreshLookup>> RefreshDatasets(IDatasetRequest[] requests, CancellationToken cancellationToken)
    {
        OnDemandRefreshStrategy refreshStrategy = new(this.logger, this.powerBIService, this.capacityAssignment);

        return await refreshStrategy.RefreshDatasets(requests, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IList<RefreshLookup>> RefreshDatasets(Guid accountId, CancellationToken cancellationToken)
    {
        ProfileKey profileKey = new(accountId);
        IProfileModel profile = await this.profileCommand.Get(profileKey, cancellationToken);
        Groups workspaces = await this.powerBIService.GetWorkspaces(profile.Id, cancellationToken);
        IEnumerable<IDatasetRequest>[] allDatasetRequests = await Task.WhenAll(workspaces.Value.Select(async workspace =>
        {
            // datasets in this workspace
            Datasets datasets = await this.powerBIService.GetDatasets(profile.Id, workspace.Id, cancellationToken);

            return datasets.Value
            .Where(dataset => AllowedDatasets().Contains(dataset.Name, StringComparer.OrdinalIgnoreCase))
            .Select(dataset =>
            {
                IDatasetRequest datasetRequest = new DatasetRequest()
                {
                    DatasetId = Guid.Parse(dataset.Id),
                    ProfileId = profile.Id,
                    WorkspaceId = workspace.Id,
                };

                return datasetRequest;
            });
        }));
        IEnumerable<IDatasetRequest> flattenedDatasets = allDatasetRequests.SelectMany(x => x);

        return await this.RefreshDatasets(flattenedDatasets.ToArray(), cancellationToken);
    }

    private async Task<DatasetRefreshDetail> GetRefreshStatus(RefreshLookup refreshLookup, CancellationToken cancellationToken)
    {
        try
        {
            return await this.powerBIService.GetRefreshStatus(refreshLookup.ProfileId, refreshLookup.WorkspaceId, refreshLookup.DatasetId, refreshLookup.RefreshRequestId, cancellationToken);
        }
        catch (HttpOperationException httpEx) when (httpEx.Response.StatusCode == HttpStatusCode.NotFound)
        {
            this.logger.LogWarning($"Refresh history is not found for ProfileId={refreshLookup.ProfileId};WorkspaceId={refreshLookup.WorkspaceId};DatasetId={refreshLookup.DatasetId}.");
            return null;
        }
    }

    private static IEnumerable<string> AllowedDatasets() => allowedDatasets.Select(x => x.Name);
}
