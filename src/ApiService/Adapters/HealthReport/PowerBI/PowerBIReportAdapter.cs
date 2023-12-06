// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles powerbi report model resource conversions.
/// </summary>
[ModelAdapter(typeof(PowerBIHealthReportModel), typeof(PowerBIHealthReport))]
public class PowerBIReportAdapter : BaseModelAdapter<PowerBIHealthReportModel, PowerBIHealthReport>
{
    /// <inheritdoc />
    public override PowerBIHealthReportModel ToModel(PowerBIHealthReport resource)
    {
        return new PowerBIHealthReportModel
        {
            Properties = new Models.PowerBIHealthReportProperties
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
    public override PowerBIHealthReport FromModel(PowerBIHealthReportModel model)
    {
        return new PowerBIHealthReport
        {
            ReportKind = model.Properties.ReportKind.ToDto(),
            Name = model.Properties?.Name,
            Category = model.Properties?.Category,
            ReportStatus = model.Properties.ReportStatus.ToDto(),
            ReportType = model.Properties.ReportType.ToDto(),
            Id = model.Properties.Id,
            Properties = new PowerBIHealthReportProperties
            {
                EmbedLink = model.Properties.EmbedLink,
                Description = model.Properties?.Description,
                DatasetId = model.Properties.DatasetId,
            }
        };
    }
}
