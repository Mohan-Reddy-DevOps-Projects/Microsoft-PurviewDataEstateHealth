// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Health trend model to DTO adapter.
/// </summary>
[ModelAdapter(typeof(IHealthTrendModel), typeof(HealthTrend))]
public class HealthTrendAdapter : BaseModelAdapter<IHealthTrendModel, HealthTrend>
{
    /// <inheritdoc />
    public override HealthTrend FromModel(IHealthTrendModel model)
    {
        var trendValuesListDTO = new List<DataTransferObjects.TrendValue>();
        foreach (var trendValue in model.TrendValuesList)
        {
            trendValuesListDTO.Add(new DataTransferObjects.TrendValue
            {
                CaptureDate = trendValue.CaptureDate,
                Value = trendValue.Value,
            });
        }

        return new HealthTrend
        {
            Kind = model.Kind.ToDto(),
            Description = model.Description,
            Duration = model.Duration,
            Unit = model.Unit,
            Delta = model.Delta,
            TrendValuesList = trendValuesListDTO,
        };
    }

    /// <inheritdoc />
    public override IHealthTrendModel ToModel(HealthTrend healthTrendDTO)
    {
        var trendValuesListModel = new List<TrendValue>();
        foreach (var trendValue in healthTrendDTO.TrendValuesList)
        {
            trendValuesListModel.Add(new TrendValue
            {
                CaptureDate = trendValue.CaptureDate,
                Value = trendValue.Value,
            });
        }

        return new HealthTrendModel
        {
            Kind = healthTrendDTO.Kind.ToModel(),
            Description = healthTrendDTO.Description,
            Duration = healthTrendDTO.Duration,
            Unit = healthTrendDTO.Unit,
            Delta = healthTrendDTO.Delta,
            TrendValuesList = trendValuesListModel,
        };
    }
}
