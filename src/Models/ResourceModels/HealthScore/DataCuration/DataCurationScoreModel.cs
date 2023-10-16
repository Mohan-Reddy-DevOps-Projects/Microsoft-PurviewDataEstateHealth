// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Data curation Score model.
/// </summary>
[HealthScoreModel(HealthScoreKind.DataCuration)]
public class DataCurationScoreModel : HealthScoreModel<DataCurationScoreProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCurationScoreModel"/> class.
    /// </summary>
    public DataCurationScoreModel()
        : base()
    {
    }
}
