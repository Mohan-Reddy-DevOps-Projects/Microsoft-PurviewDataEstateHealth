#nullable enable

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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
        get => this.GetPropertyValue<DateTime?>(keyStartTime);
        set => this.SetPropertyValue(keyStartTime, value?.GetDateTimeStr());
    }

    [EntityProperty(keyEndTime)]
    public DateTime? EndTime
    {
        get => this.GetPropertyValue<DateTime?>(keyEndTime);
        set => this.SetPropertyValue(keyEndTime, value?.GetDateTimeStr());
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

    private IEnumerable<int>? hours;

    [EntityProperty(keyHours)]
    public IEnumerable<int> Hours
    {
        get => this.hours ??= (this.GetPropertyValues<int>(keyHours) ?? []);
        set 
        {
            this.hours = value;
            this.SetPropertyValue(keyHours, value);
        }
    }

    private IEnumerable<int>? minutes;

    [EntityProperty(keyMinutes)]
    public IEnumerable<int> Minutes
    {
        get => this.minutes ??= (this.GetPropertyValues<int>(keyMinutes) ?? []);
        set
        {
            this.minutes = value;
            this.SetPropertyValue(keyMinutes, value);
        }
    }

    private IEnumerable<int>? monthDays;

    [EntityProperty(keyMonthDays)]
    public IEnumerable<int> MonthDays
    {
        get => this.monthDays ??= (this.GetPropertyValues<int>(keyMonthDays) ?? []);
        set
        {
            this.monthDays = value;
            this.SetPropertyValue(keyMonthDays, value);
        }
    }

    private IEnumerable<int>? weekDays;

    [EntityProperty(keyWeekDays)]
    public IEnumerable<int> WeekDays
    {
        get => this.weekDays ??= (this.GetPropertyValues<int>(keyWeekDays) ?? []);
        set
        {
            this.weekDays = value;
            this.SetPropertyValue(keyWeekDays, value);
        }
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
