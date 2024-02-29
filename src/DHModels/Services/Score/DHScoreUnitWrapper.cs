namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;

public class DHScoreUnitWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyAssessmentRuleId = "assessmentRuleId";
    private const string keyScore = "score";

    public DHScoreUnitWrapper() : this([]) { }

    [EntityProperty(keyAssessmentRuleId)]
    public string AssessmentRuleId
    {
        get => this.GetPropertyValue<string>(keyAssessmentRuleId);
        set => this.SetPropertyValue(keyAssessmentRuleId, value);
    }

    [EntityProperty(keyScore)]
    public double Score
    {
        get => this.GetPropertyValue<double>(keyScore);
        set => this.SetPropertyValue(keyScore, value);
    }
}
