// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Partner configuration.
/// </summary>
public class PartnerConfiguration : BaseCertificateConfiguration
{
    /// <summary>
    /// Gets or sets the enabled partners.
    /// </summary>
    public string EnabledPartners { get; set; }

    /// <summary>
    /// Gets or sets the collection.
    /// </summary>
    public string Collection { get; set; }
}
