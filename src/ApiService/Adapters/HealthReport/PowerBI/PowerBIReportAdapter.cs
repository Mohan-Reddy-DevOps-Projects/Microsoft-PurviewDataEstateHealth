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
    public override PowerBIHealthReport FromModel(PowerBIHealthReportModel model)
    {
        return new PowerBIHealthReport
        {
            ReportKind = model.Properties.ReportKind.ToDto(),
            Properties = new PowerBIHealthReportProperties
            {
                EmbedLink = model.Properties.EmbedLink,
                Name = model.Properties.Name,
                Category = model.Properties?.Category,
                Description = model.Properties?.Description,
                ReportStatus = model.Properties.ReportStatus.ToDto(),
                ReportType = model.Properties.ReportType.ToDto(),
                LastRefreshedAt = model.Properties.LastRefreshedAt,
                Id = model.Properties.Id,
                CreatedAt = model.Properties.CreatedAt,
                CreatedBy = model.Properties.CreatedAt,
                DatasetId = model.Properties.DatasetId,
                WorkspaceId = model.Properties.WorkspaceId,
                ModifiedAt = model.Properties.ModifiedAt,
                ModifiedBy = model.Properties.ModifiedBy
            }
        };
    }
}
