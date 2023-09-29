// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.PowerBI.Api.Models.Credentials;
using Microsoft.Rest;

/// <summary>
/// Power BI service
/// </summary>
internal class PowerBIService : IPowerBIService
{
    private readonly PowerBIFactory powerBIFactory;
    private readonly PowerBIAuthConfiguration authConfiguration;
    private readonly IDataEstateHealthLogger logger;

    private const string Tag = nameof(PowerBIService);

    public PowerBIService(
        PowerBIFactory powerBIFactory,
        IOptions<PowerBIAuthConfiguration> authConfiguration,
        IDataEstateHealthLogger logger)
    {
        this.powerBIFactory = powerBIFactory;
        this.authConfiguration = authConfiguration.Value;
        this.logger = logger;
    }

    /// <summary>
    /// Initialize the Power BI client
    /// </summary>
    /// <returns></returns>
    public async Task Initialize()
    {
        if (this.authConfiguration.Enabled)
        {
            await this.powerBIFactory.GetClientAsync(CancellationToken.None);
        }
    }
}
