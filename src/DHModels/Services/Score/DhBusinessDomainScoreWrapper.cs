namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;

[EntityWrapper(DhScoreBaseWrapperDerivedTypes.BusinessDomainScore, EntityCategory.Score)]
public sealed class DhBusinessDomainScoreWrapper(JObject jObject) : DHScoreBaseWrapper(jObject)
{
    private const string KeyBusinessDomainId = "businessDomainId";
    private const string KeyBusinessDomainCriticalDataElementCount = "businessDomainCriticalDataElementCount";

    public DhBusinessDomainScoreWrapper() : this([])
    {
        this.Type = DhScoreBaseWrapperDerivedTypes.BusinessDomainScore;
    }

    [EntityTypeProperty(KeyBusinessDomainId)]
    public string BusinessDomainId
    {
        get => this.GetTypePropertyValue<string>(KeyBusinessDomainId);
        set => this.SetTypePropertyValue(KeyBusinessDomainId, value);
    }

    [EntityTypeProperty(KeyBusinessDomainCriticalDataElementCount)]
    public int? BusinessDomainCriticalDataElementCount
    {
        get => this.GetTypePropertyValue<int>(KeyBusinessDomainCriticalDataElementCount);
        set => this.SetTypePropertyValue(KeyBusinessDomainCriticalDataElementCount, value);
    }
} 