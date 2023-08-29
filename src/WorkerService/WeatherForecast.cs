// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

/// <summary>
/// A sample class.
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// A sample property.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// A sample property.
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// A sample property.
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// A sample property.
    /// </summary>
    public string? Summary { get; set; }
}
