// <copyright file="DHControlController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/settings")]
public class DHSettingsController(
    DHStorageConfigService dhStorageConfigService,
    IRequestHeaderContext requestHeaderContext,
    IDataEstateHealthRequestLogger logger,
    IAccountExposureControlConfigProvider exposureControl) : DataPlaneController
{
    [HttpGet]
    [Route("storageConfig")]
    public async Task<ActionResult> GetStorageConfigAsync()
    {
        using (logger.LogElapsed($"Get storage config."))
        {
            this.CheckECEnabled();
            var accountId = this.GetAccountId();
            var storageConfig = await dhStorageConfigService.GetStorageConfig(accountId).ConfigureAwait(false);
            return this.Ok(storageConfig.JObject);
        }
    }

    [HttpPost]
    [Route("storageConfig")]
    public async Task<ActionResult> UpdateStorageConfigAsync(
    [FromBody] JObject payload)
    {
        using (logger.LogElapsed($"Update storage config."))
        {
            this.CheckECEnabled();
            if (payload == null)
            {
                throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
            }
            var accountId = this.GetAccountId();
            var entity = DHStorageConfigBaseWrapper.Create(payload);
            var result = await dhStorageConfigService.UpdateStorageConfig(entity, accountId).ConfigureAwait(false);
            return this.Created(new Uri($"{this.Request.GetEncodedUrl()}"), result.JObject);
        }
    }

    [HttpGet]
    [Route("storageConfig/mitoken")]
    public async Task<ActionResult> GetMITokenAsync()
    {
        using (logger.LogElapsed($"Generate MI token."))
        {
            this.CheckECEnabled();
            var accountId = this.GetAccountId();
            var token = await dhStorageConfigService.GetPurviewMIToken(accountId).ConfigureAwait(false);
            return this.Ok(new JObject { ["token"] = token });
        }
    }

    [HttpPost]
    [Route("storageConfig/connectivity")]
    public async Task<ActionResult> TestStorageConfigConnectivityAsync(
    [FromBody] JObject payload)
    {
        using (logger.LogElapsed($"BYOC connectivity test."))
        {
            this.CheckECEnabled();
            if (payload == null)
            {
                throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
            }

            var accountId = this.GetAccountId();
            var entity = DHStorageConfigBaseWrapper.Create(payload);
            var succeeded = false;
            var message = string.Empty;
            try
            {
                await dhStorageConfigService.TestStorageConnection(entity, accountId).ConfigureAwait(false);
                succeeded = true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Fail to test storage connection. AccountId: {accountId}", ex);
                message = ex.Message;
            }

            return this.Ok(new JObject { ["succeeded"] = succeeded, ["message"] = message });
        }
    }


    private void CheckECEnabled()
    {
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        if (!exposureControl.IsDataGovUsageSettingsEnabled(accountId, string.Empty, tenantId))
        {
            throw new EntityForbiddenException();
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
