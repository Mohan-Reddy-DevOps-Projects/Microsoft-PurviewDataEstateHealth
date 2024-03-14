namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

[EntityWrapper(EntityCategory.Score)]
public abstract class DHScoreBaseWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHScoreBaseWrapper>(jObject)
{
    private const string keyTime = "time";
    private const string keyControlId = "controlId";
    private const string keyControlGroupId = "controlGroupId";
    private const string keyComputingJobId = "computingJobId";
    private const string keyScheduleRunId = "scheduleRunId";
    private const string keyScore = "scores";
    private const string keyAggregatedScore = "aggregatedScore";

    public DHScoreBaseWrapper() : this([]) { }

    public static DHScoreBaseWrapper Create(JObject jObject)
    {
        return EntityWrapperHelper.CreateEntityWrapper<DHScoreBaseWrapper>(EntityCategory.Score, EntityWrapperHelper.GetEntityType(jObject), jObject);
    }

    [EntityProperty(keyTime)]
    public DateTime Time
    {
        get => this.GetPropertyValue<DateTime>(keyTime);
        set => this.SetPropertyValue(keyTime, value);
    }

    [EntityProperty(keyControlId)]
    public string ControlId
    {
        get => this.GetPropertyValue<string>(keyControlId);
        set => this.SetPropertyValue(keyControlId, value);
    }

    [EntityProperty(keyControlGroupId)]
    public string ControlGroupId
    {
        get => this.GetPropertyValue<string>(keyControlGroupId);
        set => this.SetPropertyValue(keyControlGroupId, value);
    }

    [EntityProperty(keyComputingJobId)]
    public string ComputingJobId
    {
        get => this.GetPropertyValue<string>(keyComputingJobId);
        set => this.SetPropertyValue(keyComputingJobId, value);
    }

    [EntityRequiredValidator]
    [EntityProperty(keyScheduleRunId)]
    public string ScheduleRunId
    {
        get => this.GetPropertyValue<string>(keyScheduleRunId);
        set => this.SetPropertyValue(keyScheduleRunId, value);
    }

    private IEnumerable<DHScoreUnitWrapper>? scores;

    [EntityProperty(keyScore)]
    public IEnumerable<DHScoreUnitWrapper> Scores
    {
        get => this.scores ??= this.GetPropertyValueAsWrappers<DHScoreUnitWrapper>(keyScore);
        set
        {
            this.SetPropertyValueFromWrappers(keyScore, value);
            this.scores = value;
        }
    }

    [EntityProperty(keyAggregatedScore)]
    public double AggregatedScore
    {
        get => this.GetPropertyValue<double>(keyAggregatedScore);
        set => this.SetPropertyValue(keyAggregatedScore, value);
    }
}

public static class DHScoreBaseWrapperDerivedTypes
{
    public const string DataProductScore = "DataProductScore";
}
