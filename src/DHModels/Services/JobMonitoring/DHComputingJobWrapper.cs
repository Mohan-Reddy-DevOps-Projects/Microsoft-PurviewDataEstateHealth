namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Newtonsoft.Json.Linq;
using System;

[CosmosDBContainer("DHJob")]
public class DHComputingJobWrapper(JObject jObject) : ContainerEntityBaseWrapper(jObject)
{
    private const string keyControlId = "controlId";
    private const string keyComputingJobId = "computingJobId";
    private const string keyCreateTime = "createTime";
    private const string keyStartTime = "startTime";
    private const string keyEndTime = "endTime";
    private const string keyStatus = "status";
    private const string keyProgress = "progress";

    public DHComputingJobWrapper() : this(new JObject()) { }

    [EntityProperty(keyControlId)]
    public string ControlId
    {
        get => this.GetPropertyValue<string>(keyControlId);
        set => this.SetPropertyValue(keyControlId, value);
    }

    [EntityProperty(keyComputingJobId)]
    public string ComputingJobId
    {
        get => this.GetPropertyValue<string>(keyComputingJobId);
        set => this.SetPropertyValue(keyComputingJobId, value);
    }

    [EntityProperty(keyCreateTime)]
    public DateTime CreateTime
    {
        get => this.GetPropertyValue<DateTime>(keyCreateTime);
        set => this.SetPropertyValue(keyCreateTime, value);
    }

    [EntityProperty(keyStartTime)]
    public DateTime StartTime
    {
        get => this.GetPropertyValue<DateTime>(keyStartTime);
        set => this.SetPropertyValue(keyStartTime, value);
    }

    [EntityProperty(keyEndTime)]
    public DateTime EndTime
    {
        get => this.GetPropertyValue<DateTime>(keyEndTime);
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
