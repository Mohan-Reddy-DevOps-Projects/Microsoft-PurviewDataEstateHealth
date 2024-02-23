// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DataQuality;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

/// <summary>
/// Token controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DataQuality)]
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/dataquality")]
public class InternalDataQualityController : Controller
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly IRequestHeaderContext requestHeaderContext;

    public InternalDataQualityController(
        IRequestHeaderContext requestHeaderContext,
        IProcessingStorageManager processingStorageManager)
    {
        this.requestHeaderContext = requestHeaderContext;
        this.processingStorageManager = processingStorageManager;
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

        var storageSasRequest = new StorageSasRequest()
        {
            Path = "/",
            Permissions = "rl", // Only read permissions
            TimeToLive = TimeSpan.FromHours(DataEstateHealthConstants.SAS_TOKEN_EXPIRATION_HOURS)
        };

        var accountModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);

        var uri = await this.processingStorageManager.GetProcessingStorageSasUri(
            accountModel,
            storageSasRequest,
            accountModel.CatalogId.ToString(),
            CancellationToken.None);

        var uriStr = uri.ToString();
        var delimiterIndex = uriStr.IndexOf("?");

        return this.Ok(new JObject()
        {
            { "sasToken", uriStr.Substring(delimiterIndex + 1) }
        });
    }
}
