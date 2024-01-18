// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.Purview.DataGovernance.Reporting.Services;
using Microsoft.Rest;

internal sealed class OnDemandRefreshStrategy
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IPowerBIService powerBIService;
    private readonly CapacityProvider capacityAssignment;

    public OnDemandRefreshStrategy(IDataEstateHealthRequestLogger logger, IPowerBIService powerBIService, CapacityProvider capacityAssignment)
    {
        this.logger = logger;
        this.powerBIService = powerBIService;
        this.capacityAssignment = capacityAssignment;
    }

    public async Task<RefreshLookup[]> RefreshDatasets(IEnumerable<IDatasetRequest> datasets, CancellationToken cancellationToken)
    {
        RefreshLookup[] response = await Task.WhenAll(datasets.Select(async datasetRequest => await this.OnDemandRefreshDataset(datasetRequest, cancellationToken)));

        return response.Where(x => x != null).ToArray();
    }

    private async Task<RefreshLookup> OnDemandRefreshDataset(IDatasetRequest datasetRequest, CancellationToken cancellationToken)
    {
        try
        {
            Guid refreshRequestId;
            DatasetRefreshRequest refreshRequest = new()
            {
                Type = DatasetRefreshType.Full,
                CommitMode = DatasetCommitMode.Transactional,
                ApplyRefreshPolicy = false,
                NotifyOption = NotifyOption.NoNotification,
            };
            Refreshes refreshHistory = await this.powerBIService.GetRefreshHistory(datasetRequest.ProfileId, datasetRequest.WorkspaceId, datasetRequest.DatasetId, cancellationToken, 1);
            IEnumerable<Refresh> incompleteRefreshes = refreshHistory.Value.Where(x => x.EndTime is null);
            if (incompleteRefreshes.Any())
            {
                this.logger.LogWarning($"Active refresh in progress. Skip refresh of dataset.");
                return null;
            }
            Capacity capacity = await this.capacityAssignment.Get(datasetRequest.ProfileId, cancellationToken);
            using (HttpResponseMessage response = await this.powerBIService.OnDemandRefresh(datasetRequest.ProfileId, datasetRequest.WorkspaceId, datasetRequest.DatasetId, refreshRequest, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                this.logger.LogInformation($"Created on demand refresh for dataset={datasetRequest}; CapacitySku={capacity.Sku}; CapacityName={capacity.DisplayName}; CapacityId={capacity.Id}; CapacityState={capacity.State}");
                response.Headers.TryGetValues("RequestId", out IEnumerable<string> requestIds);
                refreshRequestId = Guid.Parse(requestIds.First());
            }
            return new RefreshLookup()
            {
                DatasetId = datasetRequest.DatasetId,
                ProfileId = datasetRequest.ProfileId,
                RefreshRequestId = refreshRequestId,
                WorkspaceId = datasetRequest.WorkspaceId,
            };
        }
        catch (HttpOperationException httpEx) when (httpEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            this.logger.LogWarning($"Dataset={datasetRequest} not found. Skip refresh of dataset.");
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to refresh dataset={datasetRequest};", ex);
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_DatasetRefreshFailed.Code, "Failed to refresh dataset.").ToException();
        }
    }
}
