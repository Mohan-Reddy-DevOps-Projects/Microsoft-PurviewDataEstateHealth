// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// DataQuality health control model.
/// </summary>
[HealthControlModel(HealthControlKind.DataQuality)]
public class DataQualityHealthControlModel : HealthControlModel<DataQualityHealthControlProperties>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataQualityHealthControlModel"/> class.
    /// </summary>
    public DataQualityHealthControlModel()
        : base()
    {
    }
}
