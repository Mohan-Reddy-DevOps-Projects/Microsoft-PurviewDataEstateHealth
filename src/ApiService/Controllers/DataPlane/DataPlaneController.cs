// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using AspNetCore.Authentication.Certificate;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Base controller for data plane certificate authentication.
/// </summary>
[CertificateConfig(CertificateSet.DataPlane)]
[Authorize(
    Policy = "ClientCertOnly",
    AuthenticationSchemes = S2SAuthenticationDefaults.AuthenticationScheme + "," + CertificateAuthenticationDefaults.AuthenticationScheme
)]
public abstract class DataPlaneController : Controller;
