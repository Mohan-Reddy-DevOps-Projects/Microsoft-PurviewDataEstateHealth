namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

[EntityWrapper(DHAssessmentAggregationBaseWrapperDerivedTypes.Simple, EntityCategory.Assessment)]
public class DHAssessmentSimpleAggregationWrapper(JObject jObject) : DHAssessmentAggregationBaseWrapper(jObject)
{
    private const string keyAggregationType = "aggregationType";

    public DHAssessmentSimpleAggregationWrapper() : this([]) { }

    [EntityTypeProperty(keyAggregationType)]
    [EntityRequiredValidator]
    public DHAssessmentSimpleAggregationType? AggregationType
    {
        get
        {
            var enumStr = this.GetTypePropertyValue<string>(keyAggregationType);
            return Enum.TryParse<DHAssessmentSimpleAggregationType>(enumStr, true, out var result) ? result : null;
        }
        set => this.SetTypePropertyValue(keyAggregationType, value?.ToString());
    }
}
