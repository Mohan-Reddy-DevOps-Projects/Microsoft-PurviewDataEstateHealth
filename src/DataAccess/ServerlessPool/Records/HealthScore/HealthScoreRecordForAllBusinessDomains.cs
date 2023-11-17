// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for health scores for al business domains.
/// </summary>
public class HealthScoreRecordForAllBusinessDomains
{
    /// <summary>
    /// Score kind.
    /// </summary>
    [DataColumn("ScoreKind")]
    public string Kind { get; set; }

    /// <summary>
    /// Health score name.
    /// </summary>
    [DataColumn("Name")]
    public string Name { get; set; }

    /// <summary>
    /// Health score description.
    /// </summary>
    [DataColumn("Description")]
    public string Description { get; set; }

    /// <summary>
    /// Report Id.
    /// </summary>
    [DataColumn("ReportId")]
    public Guid ReportId { get; set; }

    /// <summary>
    /// Health score actual value.
    /// </summary>
    [DataColumn("ActualValue")]
    public float ActualValue { get; set; }

    /// <summary>
    /// Health score target value.
    /// </summary>
    [DataColumn("TargetValue")]
    public float TargetValue { get; set; }

    /// <summary>
    /// Health score measure unit.
    /// </summary>
    [DataColumn("MeasureUnit")]
    public string MeasureUnit { get; set; }
}
