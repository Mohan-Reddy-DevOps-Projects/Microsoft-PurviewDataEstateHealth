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
                Name = resource.Name,
                Category = resource.Category,
                Description = resource.Properties?.Description,
                ReportStatus = resource.ReportStatus.ToModel(),
                ReportType = resource.ReportType.ToModel(),
                ReportKind = resource.ReportKind.ToModel(),
                Id = resource.Id,
            },
        };
    }

    /// <inheritdoc />
    public override LegacyHealthReport FromModel(LegacyHealthReportModel model)
    {
        return new LegacyHealthReport
        {
            ReportKind = model.Properties.ReportKind.ToDto(),
            Name = model.Properties.Name,
            Category = model.Properties?.Category,
            ReportStatus = model.Properties.ReportStatus.ToDto(),
            ReportType = model.Properties.ReportType.ToDto(),
            Id = model.Properties.Id,
            Properties = new LegacyHealthReportProperties
            {
                EmbedLink = model.Properties.EmbedLink,
                Description = model.Properties?.Description,
            }
        };
    }
}
