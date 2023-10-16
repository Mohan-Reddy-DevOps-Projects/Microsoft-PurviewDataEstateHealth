// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Data quality score model.
/// </summary>
[HealthScoreModel(HealthScoreKind.DataQuality)]
public class DataQualityScoreModel : HealthScoreModel<GovernanceAndQualityScoreProperties>
{
}
