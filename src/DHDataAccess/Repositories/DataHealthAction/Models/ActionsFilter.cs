namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;

using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System;
using System.Collections.Generic;

public class ActionsFilter
{
    public List<DataHealthActionStatus>? Status { get; set; }
    public List<string>? DomainIds { get; set; }
    public List<string>? AssignedTo { get; set; }
    public List<string>? FindingTypes { get; set; }
    public List<string>? FindingSubTypes { get; set; }
    public List<string>? FindingNames { get; set; }
    public DataHealthActionSeverity? Severity { get; set; }
    public DataHealthActionTargetEntityType? TargetEntityType { get; set; }
    public List<string>? TargetEntityIds { get; set; }
    public CreateTimeRangeFilter? CreateTimeRange { get; set; }
    public Dictionary<DataHealthActionTargetEntityType, List<Obligation>>? PermissionObligations { get; set; }
}

public class CreateTimeRangeFilter
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}