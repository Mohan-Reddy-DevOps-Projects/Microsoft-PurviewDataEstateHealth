namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class DHRawScore(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyEntityId = "entityId";
    private const string keyScores = "scores";

    public DHRawScore() : this([]) { }

    [EntityProperty(keyEntityId)]
    public string EntityId
    {
        get => this.GetPropertyValue<string>(keyEntityId);
        set => this.SetPropertyValue(keyEntityId, value);
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