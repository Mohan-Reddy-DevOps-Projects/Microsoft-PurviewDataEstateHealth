namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;
using System;

public class RequestContext : IRequestContext
{
    /// <summary>
    /// For Serialization.
    /// </summary>
    public RequestContext()
    {
        
    }

    protected RequestContext(IRequestContext other)
    {
        this.AccountName = other.AccountName;
        this.AccountObjectId = other.AccountObjectId;
        this.AccountResourceId = other.AccountResourceId;
        this.ApiVersion = other.ApiVersion;
        this.CatalogId = other.CatalogId;
        this.CorrelationId = other.CorrelationId;
        this.TenantId = other.TenantId;
        this.ClientIpAddress = other.ClientIpAddress;
        this.ClientObjectId = other.ClientObjectId;
    }

    public IRequestContext WithCallbackContext(CallbackRequestContext callbackContext)
    {
        return new RequestContext(this)
        {
            AccountName = callbackContext.AccountName,
            AccountObjectId = callbackContext.AccountObjectId,
            AccountResourceId = callbackContext.AccountResourceId,
            ApiVersion = callbackContext.ApiVersion,
            CatalogId = callbackContext.CatalogId,
            CorrelationId = callbackContext.CorrelationId,
            TenantId = callbackContext.TenantId,
            ClientObjectId = callbackContext.ClientObjectId,
            ClientIpAddress = callbackContext.ClientIpAddress
        };
    }

    /// <summary>
    /// Set Correlation Id override
    /// </summary>
    /// <param name="correlationId"></param>
    public void SetCorrelationIdInRequestContext(string correlationId)
    {
        this.CorrelationId = correlationId;
    }

    [JsonProperty(PropertyName = "apiVersion")]
    public string ApiVersion { get; set; }


    public string AccountName { get; set; }

    public Guid AccountObjectId { get; set; }

    public string AccountResourceId { get; set; }

    public string CatalogId { get; set; }

    public Guid TenantId { get; set; }

    public string CorrelationId { get; set; }

    public string ClientObjectId { get; set; }

    public string ClientIpAddress { get; set; }
}
