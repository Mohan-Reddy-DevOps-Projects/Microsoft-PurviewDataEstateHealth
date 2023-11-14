// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Handles data quality score model and resource conversions.
/// </summary>
[ModelAdapter(typeof(DataQualityScoreModel), typeof(DataQualityScore))]
public class DataQualityHealthScoreAdapter : BaseModelAdapter<DataQualityScoreModel, DataQualityScore>
{
    /// <inheritdoc />
    public override DataQualityScoreModel ToModel(DataQualityScore resource)
    {
        var performanceIndicatorRulesListModel = new List<Models.PerformanceIndicatorRules>();
        foreach (var rule in resource.Properties.PerformanceIndicatorRules)
        {
            performanceIndicatorRulesListModel.Add(new Models.PerformanceIndicatorRules
            {
                RuleOrder = rule.RuleOrder,
                MinValue = rule.MinValue,
                MaxValue = rule.MaxValue,
                DefaultColor = rule.DefaultColor,
                DisplayText = rule.DisplayText
            });
        }
        return new DataQualityScoreModel
        {
            Properties = new Models.GovernanceAndQualityScoreProperties
            {
                ScoreKind = resource.ScoreKind.ToModel(),
                Name = resource.Name,
                Description = resource.Description,
                ReportId = resource.ReportId,
                ActualValue = resource.Properties.ActualValue,
                TargetValue = resource.Properties.TargetValue,
                MeasureUnit = resource.Properties.MeasureUnit,
                PerformanceIndicatorRules = performanceIndicatorRulesListModel
            },
        };
    }

    /// <inheritdoc />
    public override DataQualityScore FromModel(DataQualityScoreModel model)
    {
        var performanceIndicatorRulesListDTO = new List<PerformanceIndicatorRules>();

        if (model.Properties.PerformanceIndicatorRules != null)
        {
            foreach (var rule in model.Properties.PerformanceIndicatorRules)
            {
                performanceIndicatorRulesListDTO.Add(new PerformanceIndicatorRules
                {
                    RuleOrder = rule.RuleOrder,
                    MinValue = rule.MinValue,
                    MaxValue = rule.MaxValue,
                    DefaultColor = rule.DefaultColor,
                    DisplayText = rule.DisplayText,
                });
            }
        }

        return new DataQualityScore
        {
            ScoreKind = model.Properties.ScoreKind.ToDto(),
            Name = model.Properties.Name,
            Description = model.Properties.Description,
            ReportId = model.Properties.ReportId,
            Properties = new DataQualityScoreProperties
            {
                MeasureUnit = model.Properties.MeasureUnit,
                PerformanceIndicatorRules = performanceIndicatorRulesListDTO,
                ActualValue = model.Properties.ActualValue,
                TargetValue = model.Properties.TargetValue
            }
        };
    }
}
