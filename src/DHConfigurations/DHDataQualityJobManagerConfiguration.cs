// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHConfigurations
{
    /// <summary>
    /// Configuration for DQMJ service
    /// </summary>
    public class DHDataQualityJobManagerConfiguration : BaseDHCertificateConfiguration
    {
        public const string ConfigSectionName = "dqjmService";

        public string Endpoint { get; set; }
    }
}