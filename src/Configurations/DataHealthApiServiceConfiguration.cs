// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;
/// <summary>
/// Configurations related to metadata service
/// </summary>
public class DataHealthApiServiceConfiguration : BaseCertificateConfiguration
{
    /// <summary>
    /// Api version to use on the endpoint
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>
    /// Metadata service endpoint
    /// </summary>
    public string Endpoint { get; set; }
}
