namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;

public class DHComputingJobWrapper(JObject jObject) : ContainerEntityDynamicWrapper<DHComputingJobWrapper>(jObject)
{
    private const string keyControlId = "controlId";
    private const string keyDQJobId = "dqJobId";
    private const string keyCreateTime = "createTime";
    private const string keyStartTime = "startTime";
    private const string keyEndTime = "endTime";
    private const string keyStatus = "status";

    public static DHComputingJobWrapper Create(JObject jObject)
    {
        return new DHComputingJobWrapper(jObject);
    }

    public DHComputingJobWrapper() : this([]) { }


    [EntityRequiredValidator]
    [EntityProperty(keyDQJobId)]
    public string DQJobId
    {
        get => this.GetPropertyValue<string>(keyDQJobId);
        set => this.SetPropertyValue(keyDQJobId, value);
    }

    [EntityRequiredValidator]
    [EntityProperty(keyControlId)]
    public string ControlId
    {
        get => this.GetPropertyValue<string>(keyControlId);
        set => this.SetPropertyValue(keyControlId, value);
    }

    [EntityProperty(keyCreateTime)]
    public DateTime? CreateTime
    {
        get => this.GetPropertyValue<DateTime?>(keyCreateTime);
        set => this.SetPropertyValue(keyCreateTime, value?.GetDateTimeStr());
    }

    [EntityProperty(keyStartTime)]
    public DateTime? StartTime
    {
        get => this.GetPropertyValue<DateTime?>(keyStartTime);
        set => this.SetPropertyValue(keyStartTime, value?.GetDateTimeStr());
    }

    [EntityProperty(keyEndTime)]
    public DateTime? EndTime
    {
        get => this.GetPropertyValue<DateTime?>(keyEndTime);
        set => this.SetPropertyValue(keyEndTime, value?.GetDateTimeStr());
    }

    [EntityRequiredValidator]
    [EntityProperty(keyStatus)]
    public DHComputingJobStatus Status
    {
        get
        {
            var value = this.GetPropertyValue<string>(keyStatus);
            return Enum.TryParse(value, out DHComputingJobStatus status) ? status : DHComputingJobStatus.Unknown;
        }
        set => this.SetPropertyValue(keyStatus, value.ToString());
    }
}
