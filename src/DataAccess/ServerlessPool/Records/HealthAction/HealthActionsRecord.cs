// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for health actions.
/// </summary>
public class HealthActionsRecord : BaseRecord
{
    /// <summary>
    /// Action id.
    /// </summary>
    [DataColumn("RowId")]
    public Guid RowId { get; set; }

    /// <summary>
    /// Action id.
    /// </summary>
    [DataColumn("ActionId")]
    public Guid ActionId { get; set; }

    /// <summary>
    /// Health action display name.
    /// </summary>
    [DataColumn("DisplayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Health action description.
    /// </summary>
    [DataColumn("Description")]
    public string Description { get; set; }

    /// <summary>
    /// Health action target type.
    /// </summary>
    [DataColumn("TargetType")]
    public string TargetType { get; set; }

    /// <summary>
    /// Health action target id.
    /// </summary>
    [DataColumn("TargetId")]
    public Guid TargetId { get; set; }

    /// <summary>
    /// Health action owner id.
    /// </summary>
    [DataColumn("OwnerContactId")]
    public Guid OwnerContactId { get; set; }

    /// <summary>
    /// Health action owner name.
    /// </summary>
    [DataColumn("OwnerContactDisplayName")]
    public string OwnerContactDisplayName { get; set; }

    /// <summary>
    /// Health action control state.
    /// </summary>
    [DataColumn("HealthControlState")]
    public string HealthControlState { get; set; }

    /// <summary>
    /// Health action control name.
    /// </summary>
    [DataColumn("HealthControlName")]
    public string HealthControlName { get; set; }

    /// <summary>
    /// Health action status.
    /// </summary>
    [DataColumn("ActionStatus")]
    public string ActionStatus { get; set; }

    /// <summary>
    /// Business domain id.
    /// </summary>
    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Created At.
    /// </summary>
    [DataColumn("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last refreshed at.
    /// </summary>
    [DataColumn("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
