// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.Errors;
using HealthReportProperties = Microsoft.Azure.Purview.DataEstateHealth.Models.HealthReportProperties;

/// <summary>
/// Handles health report model resource conversions.
/// </summary>
[ModelAdapter(typeof(IHealthReportModel<HealthReportProperties>), typeof(HealthReport))]
public class HealthReportAdapter : BaseModelAdapter<IHealthReportModel<HealthReportProperties>, HealthReport>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthReportAdapter" /> class.
    /// Called by reflection from the Adapter registry.
    /// </summary>
    public HealthReportAdapter()
    { 
    }

    /// <inheritdoc />
    public override IHealthReportModel<HealthReportProperties> ToModel(HealthReport dto)
    {
        if (dto == null)
        {
            throw new ServiceError(
                    ErrorCategory.InputError,
                    ErrorCode.MissingField.Code,
                    ErrorCode.MissingField.FormatMessage("Health Report information"))
                .ToException();
        }

        IHealthReportModel<HealthReportProperties> healthReportModel =
            HealthReportModelRegistry.Instance.CreateHealthReportModelFor(dto.ReportKind.ToModel());

        healthReportModel = this.Builder.AdapterFor<IHealthReportModel<HealthReportProperties>, HealthReport>(
                healthReportModel.GetType(),
                dto.GetType())
            .ToModel<IHealthReportModel<HealthReportProperties>>(dto);

        return healthReportModel;
    }

    /// <inheritdoc />
    public override HealthReport FromModel(IHealthReportModel<HealthReportProperties> model)
    {
        dynamic adapter =
            this.Builder
                .AdapterWhereOtherIsKindOf<IHealthReportModel<HealthReportProperties>, HealthReport>(model.GetType());
        var healthReportDto = adapter.FromModel((dynamic)model) as HealthReport;

        return healthReportDto;
    }
}
