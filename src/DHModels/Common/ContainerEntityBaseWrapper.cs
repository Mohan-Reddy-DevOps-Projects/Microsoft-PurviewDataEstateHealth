namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public abstract class ContainerEntityBaseWrapper<T>(JObject jObject) : BaseEntityWrapper(jObject), IContainerEntityWrapper where T : ContainerEntityBaseWrapper<T>
{
    private const string keyId = "id";
    private const string keySystemData = "systemData";

    public ContainerEntityBaseWrapper() : this([]) { }

    [JsonProperty(keyId)] // for cosmos DB
    [EntityProperty(keyId, true)]
    [EntityIdValidator(maxLength: 1023)] // https://learn.microsoft.com/en-us/azure/cosmos-db/concepts-limits#per-item-limits
    public string Id
    {
        get => this.GetPropertyValue<string>(keyId);
        set => this.SetPropertyValue(keyId, value);
    }

    public string? TenantId { get; set; }

    public string? AccountId { get; set; }

    private SystemDataWrapper? systemData;

    [EntityProperty(keySystemData)]
    public SystemDataWrapper SystemData
    {
        get => this.systemData ??= this.GetPropertyValueAsWrapper<SystemDataWrapper>(keySystemData);
        set
        {
            this.SetPropertyValueFromWrapper(keySystemData, value);
            this.systemData = value;
        }
    }

    public virtual void OnCreate(string userId, string? id = null)
    {
        this.Id = id ?? Guid.NewGuid().ToString();

        this.SystemData = new SystemDataWrapper(userId, DateTime.UtcNow);
    }

    public virtual void OnUpdate(T existWrapper, string userId)
    {
        this.Id = existWrapper.Id;

        var existSystemData = existWrapper.SystemData ?? new SystemDataWrapper();

        existSystemData.OnModify(userId);

        this.SystemData = existSystemData;
    }
}

