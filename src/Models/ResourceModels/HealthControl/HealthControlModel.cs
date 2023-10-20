// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc />
/// <summary>
/// Health control model implementation class.
/// </summary>
public class HealthControlModel<TProperties> : IHealthControlModel<TProperties>
    where TProperties : HealthControlProperties, new()
{
    /// <summary>
    /// Instantiate instance of HealthControlModel.
    /// </summary>
    public HealthControlModel()
    {
        this.Properties = new TProperties();
    }

    /// <inheritdoc />
    public TProperties Properties { get; set; }
}
