// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health score kind.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
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
