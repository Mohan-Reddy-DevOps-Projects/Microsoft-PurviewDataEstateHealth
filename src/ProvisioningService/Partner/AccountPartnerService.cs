// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using System.Globalization;
using System.Net;

/// <summary>
/// The Partner Service implementation.
/// </summary>
internal sealed class AccountPartnerService : PartnerServiceBase, IPartnerService<AccountServiceModel, IPartnerDetails>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountPartnerService" /> class.
    /// </summary>
    /// <param name="requestHeaderContext">The request header context.</param>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The ifx logger.</param>
    public AccountPartnerService(
        IRequestHeaderContext requestHeaderContext,
        IHttpClientFactory httpClientFactory,
        IDataEstateHealthRequestLogger logger)
        : base(requestHeaderContext, logger, httpClientFactory)
    {
    }

    /// <summary>
    /// Creates the or update.
    /// </summary>
    /// <param name="partnerDetails">The partner details.</param>
    /// <param name="accountServiceModel">The account service model.</param>
    /// <param name="onSuccess">The action to run on success of the Create or Update Request.</param>
    /// <returns>The task.</returns>
    public async Task CreateOrUpdate(IPartnerDetails partnerDetails, AccountServiceModel accountServiceModel, Action<string> onSuccess)
    {
        // Build request
        using (HttpRequestMessage request = this.CreateRequest(
            HttpMethod.Put,
            partnerDetails.Endpoint,
            accountServiceModel))
        {
            // Send and validate response
            await this.SendHttpRequestMessage(
                request,
                partnerDetails.Name,
                partnerDetails.Endpoint,
                partnerDetails.ValidateResponse,
                partnerDetails.CreateOrUpdateTimeoutSeconds,
                new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent }).ConfigureAwait(false);
            onSuccess?.Invoke(partnerDetails.Name);
        }
    }

    /// <summary>
    /// Deletes the specified request URI.
    /// </summary>
    /// <param name="partnerDetails">The partner details.</param>
    /// <param name="accountServiceModel">The service model for account.</param>
    /// <param name="onSuccess">The action to run on success of the Delete Request.</param>
    /// <param name="operationType">The delete operation type.</param>
    /// <exception cref="ServiceException">Server error when trying to delete partner resource</exception>
    /// <returns>The task.</returns>
    public async Task Delete(IPartnerDetails partnerDetails, AccountServiceModel accountServiceModel, Action<string> onSuccess, OperationType operationType = OperationType.Delete)
    {
        string requestUri = string.Format(CultureInfo.InvariantCulture, $"{partnerDetails.Endpoint}/{accountServiceModel.Id}?operation={operationType}");

        // Build request
        using (var request = this.CreateRequest(
            HttpMethod.Delete,
            requestUri,
            accountServiceModel))
        {
            // Send and validate response
            await this.SendHttpRequestMessage(
                request,
                partnerDetails.Name,
                partnerDetails.Endpoint,
                partnerDetails.ValidateResponse,
                partnerDetails.DeleteTimeoutSeconds,
                new[] { HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound }).ConfigureAwait(false);
            onSuccess?.Invoke(partnerDetails.Name);
        }
    }

    /// <summary>
    /// Creates the create request.
    /// </summary>
    /// <param name="requestMethod">The request method.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="accountServiceModel">The account service model.</param>
    /// <returns>The modified http request message for create request.</returns>
    private HttpRequestMessage CreateRequest(HttpMethod requestMethod, string requestUri, AccountServiceModel accountServiceModel = null)
    {
        string requestContent = null;
        if (accountServiceModel != null)
        {
            requestContent = Rest.Serialization.SafeJsonConvert.SerializeObject(accountServiceModel);
        }

        // Create HTTP transport objects with account context, and return it
        HttpRequestMessage httpRequest = this.CreateRequest(requestMethod, requestUri, requestContent);
        this.AddAccountContext(httpRequest, accountServiceModel.Name, accountServiceModel.Id, accountServiceModel.TenantId, accountServiceModel.DefaultCatalogId);
        return httpRequest;
    }

    /// <summary>
    /// Partners the name of the component.
    /// </summary>
    /// <param name="partnerDetails">The partner details.</param>
    /// <returns>The name of the partner component.</returns>
    private static string PartnerComponentName(IPartnerDetails partnerDetails)
    {
        return $"Partner_{partnerDetails.Name.ToUpper(CultureInfo.InvariantCulture)}";
    }
}
