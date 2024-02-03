namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Newtonsoft.Json.Linq;

public class DHScoreUnitWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyName = "ruleName";
    private const string keyScore = "score";

    public DHScoreUnitWrapper() : this(new JObject()) { }

    [EntityProperty(keyName)]
    public string Name
    {
        get => this.GetPropertyValue<string>(keyName);
        set => this.SetPropertyValue(keyName, value);
    }

    [EntityProperty(keyScore)]
    public double Score
    {
        get => this.GetPropertyValue<double>(keyScore);
        set => this.SetPropertyValue(keyScore, value);
    }
}
