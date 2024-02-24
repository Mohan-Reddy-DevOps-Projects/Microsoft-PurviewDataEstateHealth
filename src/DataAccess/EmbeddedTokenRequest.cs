// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.PowerBI.Api.Models;
using System;
using System.Linq;

/// <summary>
/// Token request for PowerBI embedded.
/// </summary>
public sealed class EmbeddedTokenRequest
{
    /// <summary>
    /// The profile id
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// The dataset ids to be authorized for the token
    /// </summary>
    public Guid[] DatasetIds { get; set; }

    /// <summary>
    /// The report ids to be authorized for the token
    /// </summary>
    public Guid[] ReportIds { get; set; }

    /// <summary>
    /// The lifetime of the token in minutes
    /// </summary>
    public int LifetimeInMinutes { get; set; }

    /// <summary>
    /// Enable writer permission
    /// </summary>
    public bool EnableWriterPermission { get; set; }

    /// <summary>
    /// Convert to PowerBI SDK resource
    /// </summary>
    /// <returns></returns>
    public GenerateTokenRequestV2 ToResource()
    {
        return BuildTokenRequest(this.DatasetIds, this.ReportIds, this.LifetimeInMinutes, this.EnableWriterPermission);
    }

    /// <summary>
    /// Build the token request
    /// </summary>
    /// <param name="datasetIds"></param>
    /// <param name="reportIds"></param>
    /// <param name="lifetimeInMinutes"></param>
    /// <param name="enableWriterPermission"></param>
    /// <returns></returns>
    private static GenerateTokenRequestV2 BuildTokenRequest(Guid[] datasetIds, Guid[] reportIds, int lifetimeInMinutes, bool enableWriterPermission)
    {
        return new GenerateTokenRequestV2()
        {
            Datasets = datasetIds.Select(x => new GenerateTokenRequestV2Dataset()
            {
                Id = x.ToString(),
            }).ToList(),
            Reports = reportIds.Select(x => new GenerateTokenRequestV2Report()
            {
                Id = x,
                AllowEdit = enableWriterPermission,
            }).ToList(),
            LifetimeInMinutes = lifetimeInMinutes,
        };
    }
}
