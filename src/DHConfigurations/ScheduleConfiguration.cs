// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHConfigurations
{
    /// <summary>
    /// Configuration for a set of certificates
    /// </summary>
    public class ScheduleConfiguration : BaseDHCertificateConfiguration
    {
        public string Endpoint { get; set; }

        public string CallbackEndpoint { get; set; }
    }
}