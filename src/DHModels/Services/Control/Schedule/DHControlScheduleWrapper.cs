namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

public class DHControlScheduleStoragePayloadWrapper(JObject jObject) : ContainerEntityBaseWrapper<DHControlScheduleStoragePayloadWrapper>(jObject)
{
    public static DHControlScheduleStoragePayloadWrapper Create(JObject jObject)
    {
        return new DHControlScheduleStoragePayloadWrapper(jObject);
    }

    private const string keyType = "type";
    private const string keyProperties = "properties";

    [EntityProperty(keyType)]
    public string Type
    {
        get => this.GetPropertyValue<string>(keyType);
        set => this.SetPropertyValue(keyType, value);
    }

    private DHControlScheduleWrapper? properties;
    [EntityProperty(keyProperties)]
    public DHControlScheduleWrapper Properties
    {
        get => this.properties ??= this.GetPropertyValueAsWrapper<DHControlScheduleWrapper>(keyProperties);
        set
        {
            this.SetPropertyValueFromWrapper(keyProperties, value);
            this.properties = value;
        }
    }

    public override void OnCreate(string userId)
    {
        base.OnCreate(userId);

    }
}

public static class DHControlScheduleType
{
    public const string ControlGlobal = "ControlGlobal";
    public const string ControlNode = "ControlNode";
}

public class DHControlScheduleWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyFrequency = "frequency";
    private const string keyInterval = "interval";
    private const string keyStartTime = "startTime";
    private const string keyEndTime = "endTime";
    private const string keyTimeZone = "timeZone";
    private const string keySchedule = "schedule";

    public static DHControlScheduleWrapper Create(JObject jObject)
    {
        return new DHControlScheduleWrapper(jObject);
    }

    public DHControlScheduleWrapper() : this([]) { }

    [EntityProperty(keyFrequency)]
    public string Frequency
    {
        get => this.GetPropertyValue<string>(keyFrequency);
        set => this.SetPropertyValue(keyFrequency, value);
    }

    [EntityProperty(keyInterval)]
    public int Interval
    {
        get => this.GetPropertyValue<int>(keyInterval);
        set => this.SetPropertyValue(keyInterval, value);
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

    [EntityProperty(keyTimeZone)]
    public string TimeZone
    {
        get => this.GetPropertyValue<string>(keyTimeZone);
        set => this.SetPropertyValue(keyTimeZone, value);
    }

    private DHControlSchedulePropertiesWrapper? scheduleProperties;

    [EntityProperty(keySchedule)]
    public DHControlSchedulePropertiesWrapper Schedule
    {
        get
        {
            this.scheduleProperties ??= this.GetPropertyValueAsWrapper<DHControlSchedulePropertiesWrapper>(keySchedule);
            return this.scheduleProperties;
        }

        set
        {
            this.SetPropertyValueFromWrapper(keySchedule, value);
            this.scheduleProperties = value;
        }
    }
}

public class DHControlSchedulePropertiesWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyHours = "hours";
    private const string keyMinutes = "minutes";
    private const string keyMonthDays = "monthDays";
    private const string keyWeekDays = "weekDays";
    private const string keyMonthlyOccurrences = "monthlyOccurrences";

    public DHControlSchedulePropertiesWrapper() : this([]) { }

    [EntityProperty(keyHours)]
    public IList<int> Hours
    {
        get => this.GetPropertyValues<int>(keyHours)?.ToList() ?? [];
        set => this.SetPropertyValue(keyHours, value);
    }

    [EntityProperty(keyMinutes)]
    public IList<int> Minutes
    {
        get => this.GetPropertyValue<IList<int>>(keyMinutes);
        set => this.SetPropertyValue(keyMinutes, value);
    }

    [EntityProperty(keyMonthDays)]
    public IList<int> MonthDays
    {
        get => this.GetPropertyValue<IList<int>>(keyMonthDays);
        set => this.SetPropertyValue(keyMonthDays, value);
    }

    [EntityProperty(keyWeekDays)]
    public IList<int> WeekDays
    {
        get => this.GetPropertyValue<IList<int>>(keyWeekDays);
        set => this.SetPropertyValue(keyWeekDays, value);
    }

    private DHControlScheduleMonthlyOccurrencesWrapper? monthlyOccurrences;

    [EntityProperty(keyMonthlyOccurrences)]
    public DHControlScheduleMonthlyOccurrencesWrapper MonthlyOccurrences
    {
        get
        {
            this.monthlyOccurrences ??= this.GetPropertyValueAsWrapper<DHControlScheduleMonthlyOccurrencesWrapper>(keyMonthlyOccurrences);
            return this.monthlyOccurrences;
        }

        set
        {
            this.SetPropertyValueFromWrapper(keyMonthlyOccurrences, value);
            this.monthlyOccurrences = value;
        }
    }
}

public class DHControlScheduleMonthlyOccurrencesWrapper(JObject jObject) : BaseEntityWrapper(jObject)
{
    private const string keyDay = "day";
    private const string keyOccurrence = "occurrence";

    public DHControlScheduleMonthlyOccurrencesWrapper() : this([]) { }

    [EntityProperty(keyDay)]
    public string Day
    {
        get => this.GetPropertyValue<string>(keyDay);
        set => this.SetPropertyValue(keyDay, value);
    }

    [EntityProperty(keyOccurrence)]
    public int Occurrence
    {
        get => this.GetPropertyValue<int>(keyOccurrence);
        set => this.SetPropertyValue(keyOccurrence, value);
    }
}
