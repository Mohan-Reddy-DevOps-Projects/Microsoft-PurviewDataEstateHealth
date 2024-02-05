// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHConfigurations
{
    /// <summary>
    /// Configuration for a set of certificates
    /// </summary>
    public class DHScheduleConfiguration : BaseDHCertificateConfiguration
    {
        public const string ConfigSectionName = "scheduleService";

        public string Endpoint { get; set; }

        public string CallbackEndpoint { get; set; }
    }
}