// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc />
/// <summary>
/// Health Report model implementation class.
/// </summary>
public class HealthReportModel<TProperties> : IHealthReportModel<TProperties>
    where TProperties : HealthReportProperties, new()
{
    /// <summary>
    /// Instantiate instance of HealthReportModel.
    /// </summary>
    public HealthReportModel()
    {
        this.Properties = new TProperties();
    }

    /// <inheritdoc />
    public TProperties Properties { get; set; }
}
