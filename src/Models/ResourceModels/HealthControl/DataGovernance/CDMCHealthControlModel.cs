// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// DataGovernance health control model.
/// </summary>
[HealthControlModel(HealthControlKind.DataGovernance)]
public class DataGovernanceHealthControlModel : HealthControlModel<DataGovernanceHealthControlProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGovernanceHealthControlModel"/> class.
    /// </summary>
    public DataGovernanceHealthControlModel()
        : base()
    {
    }
}
