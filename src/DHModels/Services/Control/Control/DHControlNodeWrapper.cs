namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[EntityWrapper(DHControlBaseWrapperDerivedTypes.Node, EntityCategory.Control)]
public class DHControlNodeWrapper(JObject jObject) : DHControlBaseWrapper(jObject)
{
    public const string keyGroupId = "groupId";
    private const string keyDomains = "domains";
    private const string keySchedule = "schedule";
    private const string keyScheduleId = "scheduleId";
    public const string keyAssessmentId = "assessmentId";

    public DHControlNodeWrapper() : this([]) { }

    [EntityTypeProperty(keyGroupId)]
    public string GroupId
    {
        get => this.GetTypePropertyValue<string>(keyGroupId);
        set => this.SetTypePropertyValue(keyGroupId, value);
    }

    private IEnumerable<string>? domains;

    [EntityTypeProperty(keyDomains)]
    public IEnumerable<string>? Domains
    {
        get => this.domains ??= this.GetTypePropertyValues<string>(keyDomains);
        set
        {
            this.SetTypePropertyValue(keyDomains, value);
            this.domains = value;
        }
    }

    private DHControlScheduleWrapper? schedule;

    [EntityTypeProperty(keySchedule, true)] // TODO [YONW]: Currently we don't support schedule in control. We will make it as readonly and ignore it for now.
    public DHControlScheduleWrapper? Schedule
    {
        get
        {
            this.schedule ??= this.GetTypePropertyValueAsWrapper<DHControlScheduleWrapper>(keySchedule);
            return this.schedule;
        }
        set
        {
            this.SetTypePropertyValue(keySchedule, value);
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
