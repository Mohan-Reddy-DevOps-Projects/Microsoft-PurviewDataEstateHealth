// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHConfigurations
{
    /// <summary>
    /// Base certificate configuration containing common properties
    /// </summary>
    public abstract class BaseDHCertificateConfiguration
    {

        /// <summary>
        /// Name of the certificate
        /// </summary>
        public string CertificateName { get; set; }
    }
}