// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

/// <summary>
/// A sample controller.
/// </summary>
[ApiController]
[Route("hello")]
public class SampleController : ControllerBase
{
    private readonly SampleConfiguration sampleConfiguration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sampleConfigurationOptions"></param>
    public SampleController(IOptions<SampleConfiguration> sampleConfigurationOptions)
    {
        this.sampleConfiguration = sampleConfigurationOptions.Value;
    }

    /// <summary>
    /// A sample controller handler method.
    /// </summary>
    [HttpGet]
    public string Hello([FromQuery] string name)
    {
        return $"Hello {name} from {this.sampleConfiguration.Name}";
    }
}
