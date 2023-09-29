// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Data estate health summary page controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/weather")]
public class WeatherController : DataPlaneController
{
    /// <summary>
    /// Weather Controller constructor.
    /// </summary>
    public WeatherController()
    {
    }

    /// <summary>
    /// Gets the weather
    /// </summary>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(int), 200)]
    public IActionResult GetWeather([FromQuery(Name = "api-version")] string apiVersion)
    {
        return this.Ok(1);
    }
}
