namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

[EntityWrapper(DHControlBaseWrapperDerivedTypes.Node, EntityCategory.Control)]
public class DHControlNodeWrapper(JObject jObject) : DHControlBaseWrapper(jObject)
{
    private const string keyGroupId = "groupId";
    private const string keyDomains = "domains";
    // private const string keySchedule = "schedule";
    private const string keyAssessmentId = "assessmentId";

    public DHControlNodeWrapper() : this(new JObject()) { }

    [EntityProperty(keyGroupId)]
    public string GroupId
    {
        get => this.GetPropertyValue<string>(keyGroupId);
        set => this.SetPropertyValue(keyGroupId, value);
    }

    private IList<string>? domains;

    [EntityProperty(keyDomains)]
    public IList<string> Domains
    {
        get => this.domains ??= (this.GetPropertyValues<string>(keyDomains)?.ToList() ?? []);
        set
        {
            this.SetPropertyValue(keyDomains, value);
            this.domains = value;
        }
    }

    /*
    private DHControlSchedule? schedule;

    [EntityProperty(keySchedule)]
    public DHControlSchedule? Schedule
    {
        // TODO: [@YuanqiuLi] Change to GetPropertyValueAsWrapper when DHControlScheduleWrapper is implemented
        get => this.schedule ?? this.GetPropertyValue<DHControlSchedule>(keySchedule);
        set
        {
            this.SetPropertyValue(keySchedule, value);
            this.schedule = value;
        }
    }
    */

    [EntityProperty(keyAssessmentId)]
    public string? AssessmentId
    {
        get => this.GetPropertyValue<string>(keyAssessmentId);
        set => this.SetPropertyValue(keyAssessmentId, value);
    }
}
