﻿namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

public interface IRequestContext
{
    /// <summary>
    /// Microsoft Purview API Version
    /// </summary>
    string ApiVersion { get; }

    /// <summary>
    /// Microsoft Purview Account Name
    /// </summary>
    string AccountName { get; }

    /// <summary>
    /// Microsoft Purview Account Object Id
    /// </summary>
    Guid AccountObjectId { get; set; }

    /// <summary>
    /// Microsoft Purview Account Resource Id
    /// </summary>
    string AccountResourceId { get; }

    /// <summary>
    /// Catalog Id
    /// </summary>
    string CatalogId { get; }

    /// <summary>
    /// Tenant Id of the client
    /// </summary>
    Guid TenantId { get; set; }

    /// <summary>
    /// Request Correlation Id
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Client Object Id - used for auditing
    /// </summary>
    public string ClientObjectId { get; set; }


    /// <summary>
    /// Client IP Address - used for auditing
    /// </summary>
    public string ClientIpAddress { get; set; }

    /// <summary>
    /// Clone the request context 
    /// </summary>
    /// <param name="callbackContext"></param>
    /// <returns></returns>
    IRequestContext WithCallbackContext(CallbackRequestContext callbackContext);

    /// <summary>
    /// Set Correlation Id in Request Context
    /// </summary>
    /// <param name="correlationId"></param>
    void SetCorrelationIdInRequestContext(string correlationId);
}