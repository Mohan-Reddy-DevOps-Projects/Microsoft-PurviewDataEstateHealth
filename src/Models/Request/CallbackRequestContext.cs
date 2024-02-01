namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// NOTE: Properties of Callback Request Context are serialized along with the Callback messages. So, only select properties are part of the context here.
/// </summary>
public sealed class CallbackRequestContext : RequestContext
{
    /// <summary>
    /// For Serialization.
    /// </summary>
    public CallbackRequestContext()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public CallbackRequestContext(IRequestContext context) : base(context)
    {
    }

    /// <summary>
    /// The account id.
    /// </summary>
    [JsonProperty(PropertyName = "accountId")]
    public Guid AccountId => this.AccountObjectId;
}