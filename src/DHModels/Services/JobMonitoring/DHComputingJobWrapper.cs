namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;

public class DHComputingJobWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHComputingJobWrapper>(jObject)
{
    private const string keyControlId = "controlId";
    private const string keyComputingJobId = "computingJobId";
    private const string keyCreateTime = "createTime";
    private const string keyStartTime = "startTime";
    private const string keyEndTime = "endTime";
    private const string keyStatus = "status";
    private const string keyProgress = "progress";

    public DHComputingJobWrapper() : this([]) { }

    [EntityProperty(keyControlId)]
    public string ControlId
    {
        get => this.GetPropertyValue<string>(keyControlId);
        set => this.SetPropertyValue(keyControlId, value);
    }

    [EntityProperty(keyCreateTime)]
    public DateTime? CreateTime
    {
        get
        {
            var time = this.GetPropertyValue<DateTime>(keyCreateTime);
            return time == DateTime.MinValue ? null : time;
        }

        set => this.SetPropertyValue(keyCreateTime, value);
    }

    [EntityProperty(keyStartTime)]
    public DateTime? StartTime
    {
        get
        {
            var time = this.GetPropertyValue<DateTime>(keyStartTime);
            return time == DateTime.MinValue ? null : time;
        }

        set => this.SetPropertyValue(keyStartTime, value);
    }

    [EntityProperty(keyEndTime)]
    public DateTime? EndTime
    {
        get
        {
            var time = this.GetPropertyValue<DateTime>(keyEndTime);
            return time == DateTime.MinValue ? null : time;
        }

        set => this.SetPropertyValue(keyEndTime, value);
    }

    [EntityProperty(keyStatus)]
    public DHComputingJobStatus Status
    {
        get => this.GetPropertyValue<DHComputingJobStatus>(keyStatus);
        set => this.SetPropertyValue(keyStatus, value);
    }

    [EntityProperty(keyProgress)]
    public double? Progress
    {
        get => this.GetPropertyValue<double?>(keyProgress);
        set => this.SetPropertyValue(keyProgress, value);
    }
}
