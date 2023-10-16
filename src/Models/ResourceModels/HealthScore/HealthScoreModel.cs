// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc />
/// <summary>
/// Health score model implementation class.
/// </summary>
public class HealthScoreModel<TProperties> : IHealthScoreModel<TProperties>
    where TProperties : HealthScoreProperties, new()
{
    /// <summary>
    /// Instantiate instance of HealthScoreModel.
    /// </summary>
    public HealthScoreModel()
    {
        this.Properties = new TProperties();
    }

    /// <inheritdoc />
    public TProperties Properties { get; set; }
}
