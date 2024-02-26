namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHRawScore(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyEntityPayload = "entityPayload";
    private const string keyScores = "scores";

    public DHRawScore() : this([]) { }

    [EntityProperty(keyEntityPayload)]
    public JObject EntityPayload
    {
        get => this.GetPropertyValue<JObject>(keyEntityPayload);
        set => this.SetPropertyValue(keyEntityPayload, value);
    }

    private IEnumerable<DHScoreUnitWrapper>? scores;

    [EntityProperty(keyScores)]
    public IEnumerable<DHScoreUnitWrapper> Scores
    {
        get => this.scores ??= this.GetPropertyValueAsWrappers<DHScoreUnitWrapper>(keyScores);
        set
        {
            this.SetPropertyValueFromWrappers(keyScores, value);
            this.scores = value;
        }
    }
}