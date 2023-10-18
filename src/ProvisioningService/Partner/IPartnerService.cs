// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using System;
using System.Threading.Tasks;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// The interface for Partner Service.
/// </summary>
/// <typeparam name="TModel">The model type for which partners will be notified.</typeparam>
/// <typeparam name="TPartnerDetails">The partner details type.</typeparam>
public interface IPartnerService<TModel, TPartnerDetails>
{
    /// <summary>
    /// Notifies the partner on create or update.
    /// </summary>
    /// <param name="partnerDetails">The partner details.</param>
    /// <param name="serviceModel">The service model.</param>
    /// <param name="onSuccess">The action to run on success of the Create or Update operation.</param>
    /// <returns>The task.</returns>
    Task CreateOrUpdate(TPartnerDetails partnerDetails, TModel serviceModel, Action<string> onSuccess);

    /// <summary>
    /// Notifies the partner on delete.
    /// </summary>
    /// <param name="partnerDetails">The partner details.</param>
    /// <param name="serviceModel">The service model.</param>
    /// <param name="onSuccess">The action to run on success of the Delete operation.</param>
    /// <param name="operationType">The operation type.</param>
    /// <returns>The task.</returns>
    /// <exception cref="ServiceException">Server error when trying to delete partner resource</exception>
    Task Delete(TPartnerDetails partnerDetails, TModel serviceModel, Action<string> onSuccess, OperationType operationType = OperationType.Delete);
}
