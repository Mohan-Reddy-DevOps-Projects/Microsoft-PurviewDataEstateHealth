// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.PowerBI.Api.Models;

internal sealed class OnDemandRefreshStrategy
{
    private readonly IDataEstateHealthLogger logger;
    private readonly IPowerBIService powerBIService;
    private readonly ICapacityAssignment capacityAssignment;

    public OnDemandRefreshStrategy(IDataEstateHealthLogger logger, IPowerBIService powerBIService, ICapacityAssignment capacityAssignment)
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
            Capacity capacity = await this.capacityAssignment.GetCapacity(datasetRequest.ProfileId, cancellationToken);
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
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to refresh dataset={datasetRequest};", ex);
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_DatasetRefreshFailed.Code, "Failed to refresh dataset.").ToException();
        }
    }
}
