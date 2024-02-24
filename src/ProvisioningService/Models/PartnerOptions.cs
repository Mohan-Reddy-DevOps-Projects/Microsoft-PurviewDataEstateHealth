// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

/// <summary>
/// An object representing the options for each partner.
/// </summary>
public class PartnerOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Partner has succeeded in its callback.
    /// This property is used to skip notifying partners on retries who have already been successful.
    /// </summary>
    public bool HasSucceeded { get; set; }
}
