// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <inheritdoc />
public class DataEstateHealthOtelInstrumentation : IOtelInstrumentation, IDisposable
{
    /// <summary>
    /// Activity source - used to manually create traces
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Name of the metric
    /// </summary>
    public const string InstrumentationName = "DataEstateHealthLogEvent";

    private readonly Meter meter;

    private readonly Counter<short> counter;

    /// <summary>
    /// Instantiate instance of PurviewShareMdmInstrumentation.
    /// </summary>
    public DataEstateHealthOtelInstrumentation()
    {
        string version = typeof(DataEstateHealthOtelInstrumentation).Assembly.GetName().Version?.ToString();
        this.meter = new(InstrumentationName, version);

        this.ActivitySource = new ActivitySource(InstrumentationName, version);
        this.counter = this.meter.CreateCounter<short>(InstrumentationName);
    }

    /// <inheritdoc />
    /// <summary>
    /// Helper method to create value events 
    /// </summary>
    /// <param name="value">The value to be logged</param>
    /// <param name="dimensions">Dictionary containing the dimensions used in the metric</param>
    public void LogErrorMetricValue(short value, Dictionary<string, string> dimensions)
    {
        var tagList = new TagList();

        foreach (var dimension in dimensions)
        {
            tagList.Add(dimension.Key, SanitizeDimensionValue(dimension.Value));
        }
        this.counter.Add(value, tagList);
    }

    /// <summary>
    /// Method to fix null and long dimensions for MDM metrics
    /// </summary>
    /// <param name="value">The string to sanitize</param>
    /// <returns>The sanitized dimension</returns>
    private static string SanitizeDimensionValue(string value)
    {
        // truncate strings too long for MDM dimensions

        if (string.IsNullOrEmpty(value))
        {
            value = "_empty";
        }
        value = value[..Math.Min(value.Length, 255)];

        return value;
    }

    /// <summary>
    /// Disposes the metric
    /// </summary>
    public void Dispose()
    {
        this.ActivitySource.Dispose();
        this.meter.Dispose();
    }
}
