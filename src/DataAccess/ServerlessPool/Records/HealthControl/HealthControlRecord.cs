// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for Health Control.
/// </summary>
public class HealthControlRecord : BaseRecord
{
    /// <summary>
    /// Actual Value
    /// </summary>
    [DataColumn("ActualValue")]
    public double ActualValue { get; set; }

    /// <summary>
    /// Ownership
    /// </summary>
    [DataColumn("C2_Ownership")]
    public double C2_Ownership { get; set; }

    /// <summary>
    /// Authoritative source
    /// </summary>
    [DataColumn("C3_AuthoritativeSource")]
    public double C3_AuthoritativeSource { get; set; }

    /// <summary>
    /// Catalog
    /// </summary>
    [DataColumn("C5_Catalog")]
    public double C5_Catalog { get; set; }

    /// <summary>
    /// Classification
    /// </summary>
    [DataColumn("C6_Classification")]
    public double C6_Classification { get; set; }

    /// <summary>
    /// Access
    /// </summary>
    [DataColumn("C7_Access")]
    public double C7_Access { get; set; }

    /// <summary>
    /// Data consumption purpose
    /// </summary>
    [DataColumn("C8_DataConsumptionPurpose")]
    public double C8_DataConsumptionPurpose { get; set; }

    /// <summary>
    /// Data quality
    /// </summary>
    [DataColumn("C12_DataQuality")]
    public double C12_DataQuality { get; set; }

    /// <summary>
    /// Use
    /// </summary>
    [DataColumn("Use")]
    public double Use { get; set; }

    /// <summary>
    /// Quality
    /// </summary>
    [DataColumn("Quality")]
    public double Quality { get; set; }

    /// <summary>
    /// Metadata completeness
    /// </summary>
    [DataColumn("MetadataCompleteness")]
    public double MetadataCompleteness { get; set; }

    /// <summary>
    /// Last refreshed at
    /// </summary>
    [DataColumn("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }
}
