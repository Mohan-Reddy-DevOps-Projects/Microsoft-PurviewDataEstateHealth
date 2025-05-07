// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DataQuality;

using Asp.Versioning;
using AspNetCore.Authentication.Certificate;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

/// <summary>
/// Token controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DataQuality)]
[Authorize(
    Policy = "ClientCertOnly",
    AuthenticationSchemes = S2SAuthenticationDefaults.AuthenticationScheme + "," + CertificateAuthenticationDefaults.AuthenticationScheme
)]
[Route("/internal/dataquality")]
public class InternalDataQualityController : Controller
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly IRequestHeaderContext requestHeaderContext;
    private readonly DHStorageConfigService dhStorageConfigService;
    private readonly IDataEstateHealthRequestLogger logger;

    public InternalDataQualityController(
        IRequestHeaderContext requestHeaderContext,
        IProcessingStorageManager processingStorageManager,
        DHStorageConfigService dhStorageConfigService,
        IDataEstateHealthRequestLogger logger)
    {
        this.requestHeaderContext = requestHeaderContext;
        this.processingStorageManager = processingStorageManager;
        this.dhStorageConfigService = dhStorageConfigService;
        this.logger = logger;
    }

    [HttpPost]
    [Route("getProcessingStorageSasToken")]
    public async Task<IActionResult> GetProcessingStorageSasToken(
        [FromBody] JObject sasUriRequest)
    {
        if (!sasUriRequest.ContainsKey("accountId"))
        {
            return this.BadRequest("accountId is required in body");
        }

        var accountId = sasUriRequest["accountId"].ToString();

        var accountModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);

        if (accountModel == null)
        {
            return this.BadRequest($"Cannot find processing storage account mapping for this account id {accountId}");
        }

        StorageSasRequest storageSasRequest = new()
        {
            Resource = "c",
            Path = string.Empty,
            Permissions = "racwdlmeop",
            TimeToLive = TimeSpan.FromHours(DataEstateHealthConstants.SAS_TOKEN_EXPIRATION_HOURS),
            IsDirectory = null
        };

        var token = await this.processingStorageManager.GetSasTokenForDQ(accountModel, storageSasRequest).ConfigureAwait(false);

        return this.Ok(new JObject()
        {
            { "sasToken", token }
        });
    }

    [HttpGet]
    [Route("storageConfig")]
    public async Task<ActionResult> GetStorageConfigAsync()
    {
        using (this.logger.LogElapsed($"Get storage config internal."))
        {
            try
            {
                var accountId = this.GetAccountId();
                var storageConfig = await this.dhStorageConfigService.GetStorageConfig(accountId).ConfigureAwait(false);
                return this.Ok(storageConfig.JObject);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving storage config.");
                return this.StatusCode(500, "An error occurred while retrieving the storage configuration.");
            }
        }
    }

    private string GetAccountId()
    {
        var accountId = this.requestHeaderContext.AccountObjectId;
        if (accountId.Equals(Guid.Empty))
        {
            throw new InvalidRequestException("Empty account id in request headers.");
        }
        return accountId.ToString();
    }
}
