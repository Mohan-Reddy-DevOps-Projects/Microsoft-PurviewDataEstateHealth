namespace Microsoft.Purview.DataEstateHealth.DHModels.Common;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared;

public interface IContainerEntityWrapper
{
    public string Id { get; set; }

    public string? TenantId { get; set; }

    public string? AccountId { get; set; }

    public SystemDataWrapper SystemData { get; set; }
}
