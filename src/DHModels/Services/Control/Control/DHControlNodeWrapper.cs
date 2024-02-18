namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

[EntityWrapper(DHControlBaseWrapperDerivedTypes.Node, EntityCategory.Control)]
public class DHControlNodeWrapper(JObject jObject) : DHControlBaseWrapper(jObject)
{
    private const string keyGroupId = "groupId";
    private const string keyDomains = "domains";
    private const string keySchedule = "schedule";
    private const string keyScheduleId = "scheduleId";
    private const string keyAssessmentId = "assessmentId";

    public DHControlNodeWrapper() : this([]) { }

    [EntityTypeProperty(keyGroupId)]
    public string GroupId
    {
        get => this.GetTypePropertyValue<string>(keyGroupId);
        set => this.SetTypePropertyValue(keyGroupId, value);
    }

    private IList<string>? domains;

    [EntityTypeProperty(keyDomains)]
    public IList<string> Domains
    {
        get => this.domains ??= (this.GetTypePropertyValues<string>(keyDomains)?.ToList() ?? []);
        set
        {
            this.SetTypePropertyValue(keyDomains, value);
            this.domains = value;
        }
    }


    private DHControlScheduleWrapper? schedule;

    [EntityProperty(keySchedule)]
    public DHControlScheduleWrapper? Schedule
    {
        get
        {
            this.schedule ??= this.GetPropertyValueAsWrapper<DHControlScheduleWrapper>(keySchedule);
            return this.schedule;
        }
        set
        {
            this.SetPropertyValue(keySchedule, value);
            this.schedule = value;
        }
    }

    public string? ScheduleId
    {
        get => this.GetTypePropertyValue<string>(keyScheduleId);
        set => this.SetTypePropertyValue(keyScheduleId, value);
    }

    [EntityTypeProperty(keyAssessmentId)]
    public string? AssessmentId
    {
        get => this.GetTypePropertyValue<string>(keyAssessmentId);
        set => this.SetTypePropertyValue(keyAssessmentId, value);
    }
}
