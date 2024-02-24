// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;
using System.Collections.Generic;

/// <summary>
/// Client certificate configuration model.
/// </summary>
public class AllowListedCertificateConfiguration
{
    /// <summary>
    /// Allow listed issuer names.
    /// </summary>
    public List<string> AllowListedIssuerNames { get; set; }

    /// <summary>
    /// Allow listed data plane subject names.
    /// </summary>
    public List<string> AllowListedDataPlaneSubjectNames { get; set; }

    /// <summary>
    /// Allow listed control plane subject names.
    /// </summary>
    public List<string> AllowListedControlPlaneSubjectNames { get; set; }

    /// <summary>
    /// Allow listed data quality subject names.
    /// </summary>
    public List<string> AllowListedDataQualitySubjectNames { get; set; }

    /// <summary>
    /// Allow listed schedule service subject names.
    /// </summary>
    public List<string> AllowListedDHControlScheduleSubjectNames { get; set; }

    /// <summary>
    /// Get list of allowed subject names.
    /// </summary>
    /// <param name="certificateSet">Input certificate set.</param>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public List<string> GetAllowListedSubjectNames(CertificateSet certificateSet)
    {
        switch (certificateSet)
        {
            case CertificateSet.DataPlane:
                return this.AllowListedDataPlaneSubjectNames;
            case CertificateSet.ControlPlane:
                return this.AllowListedControlPlaneSubjectNames;
            case CertificateSet.DataQuality:
                return this.AllowListedDataQualitySubjectNames;
            case CertificateSet.DHControlSchedule:
                return this.AllowListedDHControlScheduleSubjectNames;
            default:
                throw new ServiceError(
                        ErrorCategory.ServiceError,
                        ErrorCode.Invalid_CertificateSet.Code,
                        "Invalid certificate set value.")
                    .ToException();
        }
    }
}
