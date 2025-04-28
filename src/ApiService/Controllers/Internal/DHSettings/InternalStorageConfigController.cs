#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHSettings;

using Asp.Versioning;
using AspNetCore.Authentication.Certificate;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DHSettings)]
[Authorize(
    Policy = "ClientCertOnly",
    AuthenticationSchemes = S2SAuthenticationDefaults.AuthenticationScheme + "," + CertificateAuthenticationDefaults.AuthenticationScheme
)]
[Route("/internal/settings")]
public class InternalStorageConfigController(
      DHStorageConfigService dhStorageConfigService,
    IRequestHeaderContext requestHeaderContext,
    IDataEstateHealthRequestLogger logger) : Controller
{
    [HttpGet]
    [Route("storageConfig")]
    public async Task<ActionResult> GetStorageConfigAsync()
    {
        using (logger.LogElapsed($"Get storage config."))
        {
            var accountId = this.GetAccountId();
            var storageConfig = await dhStorageConfigService.GetStorageConfig(accountId).ConfigureAwait(false);
            return this.Ok(storageConfig.JObject);
        }
    }

    [HttpGet]
    [Route("storageConfig/mitoken")]
    public async Task<ActionResult> GetMITokenAsync()
    {
        using (logger.LogElapsed($"Generate MI token."))
        {
            var accountId = this.GetAccountId();
            var token = await dhStorageConfigService.GetPurviewMIToken(accountId).ConfigureAwait(false);
            return this.Ok(new JObject { ["token"] = token });
        }
    }

    private string GetAccountId()
    {
        var accountId = requestHeaderContext.AccountObjectId;
        if (accountId.Equals(Guid.Empty))
        {
            throw new InvalidRequestException("Empty account id in request headers.");
        }
        return accountId.ToString();
    }
}
