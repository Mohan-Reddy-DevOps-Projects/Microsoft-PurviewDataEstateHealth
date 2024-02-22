namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DHScoreWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHScoreWrapper>(jObject)
{
    private const string keyTime = "time";
    private const string keyControlId = "controlId";
    private const string keyEntityDomainId = "entityDomainId";
    private const string keyControlGroupId = "controlGroupId";
    private const string keyComputingJobId = "computingJobId";
    private const string keyEntitySnapshot = "entitySnapshot";
    private const string keyScore = "scores";
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

    [EntityProperty(keyEntityDomainId)]
    public string EntityDomainId
    {
        get => this.GetPropertyValue<string>(keyEntityDomainId);
        set => this.SetPropertyValue(keyEntityDomainId, value);
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

    [EntityProperty(keyEntitySnapshot)]
    public JObject EntitySnapshot
    {
        get => this.GetPropertyValue<JObject>(keyEntitySnapshot);
        set => this.SetPropertyValue(keyEntitySnapshot, value);
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
