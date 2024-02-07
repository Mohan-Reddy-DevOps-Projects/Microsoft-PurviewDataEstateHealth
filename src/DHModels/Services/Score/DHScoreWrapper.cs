namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DHScoreWrapper(JObject jObject) : ContainerEntityBaseWrapper(jObject)
{
    private const string keyTime = "time";
    private const string keyControlId = "controlId";
    private const string keyComputingJobId = "computingJobId";
    private const string keyEntitySnapshot = "entitySnapshot";
    private const string keyScore = "score";
    private const string keyAggregatedScore = "aggregatedScore";

    public DHScoreWrapper() : this([]) { }

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

    [EntityProperty(keyComputingJobId)]
    public string ComputingJobId
    {
        get => this.GetPropertyValue<string>(keyComputingJobId);
        set => this.SetPropertyValue(keyComputingJobId, value);
    }

    [EntityProperty(keyEntitySnapshot)]
    public JObject EntitySnapshot
    {
        get => this.GetPropertyValue<JObject>(keyEntitySnapshot);
        set => this.SetPropertyValue(keyEntitySnapshot, value);
    }

    private IEnumerable<DHScoreUnitWrapper>? score;

    [EntityProperty(keyScore)]
    public IEnumerable<DHScoreUnitWrapper> Score
    {
        get => this.score ??= this.GetPropertyValueAsWrappers<DHScoreUnitWrapper>(keyScore);
        set
        {
            this.SetPropertyValueFromWrappers(keyScore, value);
            this.score = value;
        }
    }

    [EntityProperty(keyAggregatedScore)]
    public double? AggregatedScore
    {
        get => this.GetPropertyValue<double>(keyAggregatedScore);
        set => this.SetPropertyValue(keyAggregatedScore, value);
    }
}
