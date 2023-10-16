// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Data governance score model.
/// </summary>
[HealthScoreModel(HealthScoreKind.DataGovernance)]
public class DataGovernanceScoreModel : HealthScoreModel<GovernanceAndQualityScoreProperties>
{
}
