// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.PowerBI.Api.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal sealed class CapacityAssignment : ICapacityAssignment
{
    private readonly IPowerBIService powerBiService;
    private readonly IAccountExposureControlConfigProvider exposureControl;
    private readonly IDataEstateHealthRequestLogger logger;

    public CapacityAssignment(IPowerBIService powerBiService, IAccountExposureControlConfigProvider exposureControl, IDataEstateHealthRequestLogger logger)
    {
        this.powerBiService = powerBiService;
        this.exposureControl = exposureControl;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task AssignWorkspace(Guid profileId, Guid workspaceId, CancellationToken cancellationToken)
    {
        Capacity capacity = await this.GetCapacity(profileId, cancellationToken);
        this.logger.LogInformation($"Assigning capacity: Id={capacity.Id}; Name={capacity.DisplayName}; Sku={capacity.Sku}; State={capacity.State} to WorkspaceId={workspaceId}");
        await this.powerBiService.AssignWorkspaceCapacity(profileId, workspaceId, capacity.Id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Capacity> GetCapacity(Guid profileId, CancellationToken cancellationToken)
    {
        Dictionary<string, CapacityModel> capacities = this.exposureControl.GetPBICapacities();
        CapacityModel capacity = DetermineAvailableCapacity(capacities);
        Capacities pbiCapacities = await this.powerBiService.ListCapacities(profileId, cancellationToken);
        IEnumerable<Capacity> matchedCapacity = pbiCapacities.Value.Where(x => x.Id == capacity.CapacityId && x.State == CapacityState.Active).ToArray();
        if (!matchedCapacity.Any())
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_CapacityNotFound.Code, $"No available capacities are available for SKU: {capacity.SkuName}").ToException();
        }

        return matchedCapacity.First();
    }

    /// <summary>
    /// Selects the most appropriate capacity based on:
    /// 1) SKU - Free tier vs enterprise
    /// 2) Capacity utilization (TBD)
    /// </summary>
    /// <param name="capacities"></param>
    /// <returns></returns>
    private static CapacityModel DetermineAvailableCapacity(Dictionary<string, CapacityModel> capacities)
    {
        IEnumerable<KeyValuePair<string, CapacityModel>> matchingCapacities = capacities.Where(x => x.Value.SkuName.Equals("standard", StringComparison.OrdinalIgnoreCase));
        // where sku is null it is enterprise tier tier
        if (!matchingCapacities.Any())
        {
            matchingCapacities = capacities.Where(x => x.Value.SkuName.Equals(AccountSkuName.Standard, StringComparison.OrdinalIgnoreCase));
        }
        if (!matchingCapacities.Any())
        {
            throw new ServiceError(ErrorCategory.ServiceError, ErrorCode.PowerBI_CapacityNotFound.Code, $"No available capacities are available for SKU: standard").ToException();
        }

        return matchingCapacities.First().Value;
    }
}
