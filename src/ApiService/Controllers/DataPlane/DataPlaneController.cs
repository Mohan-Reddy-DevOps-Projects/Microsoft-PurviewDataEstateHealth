// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Base controller for data plane certificate authentication.
/// </summary>
[CertificateConfig(CertificateSet.DataPlane)]
[Authorize(AuthenticationSchemes = "Certificate")]
public abstract class DataPlaneController : Controller
{
}
