// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// A sample controller.
/// </summary>
[ApiController]
[Route("hello")]
public class SampleController : ControllerBase
{
    /// <summary>
    /// A sample controller handler method.
    /// </summary>
    [HttpGet]
    public string Hello([FromQuery] string name)
    {
        return $"Hello {name}";
    }
}
