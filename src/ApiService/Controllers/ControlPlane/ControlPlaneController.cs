// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// </summary>
[CertificateConfig(CertificateSet.ControlPlane)]
[Authorize(AuthenticationSchemes = "Certificate")]
public abstract class ControlPlaneController : Controller
{
}
