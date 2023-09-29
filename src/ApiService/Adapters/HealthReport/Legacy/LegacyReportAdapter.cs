// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles legacy report model resource conversions.
/// </summary>
[ModelAdapter(typeof(LegacyHealthReportModel), typeof(LegacyHealthReport))]
public class
    LegacyReportAdapter : BaseModelAdapter<LegacyHealthReportModel, LegacyHealthReport>
{
    /// <inheritdoc />
    public override LegacyHealthReportModel ToModel(LegacyHealthReport resource)
    {
        return new LegacyHealthReportModel
        {
            Properties = new Models.LegacyHealthReportProperties
            {
                EmbedLink = resource.Properties?.EmbedLink,
                Name = resource.Properties.Name,
                Category = resource.Properties?.Category,
                Description = resource.Properties?.Description,
                ReportStatus = resource.Properties.ReportStatus.ToModel(),
                ReportType = resource.Properties.ReportType.ToModel(),
                ReportKind = resource.ReportKind.ToModel(),
                LastRefreshedAt = resource.Properties.LastRefreshedAt,
                Id = resource.Properties.Id      
            },
        };
    }

    /// <inheritdoc />
    public override LegacyHealthReport FromModel(LegacyHealthReportModel model)
    {
        return new LegacyHealthReport
        {
            ReportKind = model.Properties.ReportKind.ToDto(),
            Properties = new LegacyHealthReportProperties
            {
                EmbedLink = model.Properties.EmbedLink,
                Name = model.Properties.Name,
                Category = model.Properties?.Category,
                Description = model.Properties?.Description,
                ReportStatus = model.Properties.ReportStatus.ToDto(),
                ReportType = model.Properties.ReportType.ToDto(),
                LastRefreshedAt = model.Properties.LastRefreshedAt,
                Id = model.Properties.Id
            }
        };
    }
}
