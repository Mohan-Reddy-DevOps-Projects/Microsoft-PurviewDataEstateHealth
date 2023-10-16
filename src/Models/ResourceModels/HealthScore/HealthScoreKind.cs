// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Health score kind model.
/// </summary>
public enum HealthScoreKind
{
    /// <summary>
    /// Data governance Score
    /// </summary>
    DataGovernance = 1,

    /// <summary>
    /// Data quality health score.
    /// </summary>
    DataQuality,

    /// <summary>
    /// Data curation health score.
    /// </summary>
    DataCuration
}
