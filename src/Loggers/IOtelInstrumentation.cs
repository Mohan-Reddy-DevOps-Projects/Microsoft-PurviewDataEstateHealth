// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Interface to handle OpenTelemetry instrumentation
/// </summary>
public interface IOtelInstrumentation
{
    /// <summary>
    /// Activity source - used to manually create traces
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Create value events in MDM
    /// </summary>
    /// <param name="value">The value to be logged</param>
    /// <param name="dimensions">Dictionary containing the dimensions used in the metric</param>
    void LogErrorMetricValue(short value, Dictionary<string, string> dimensions);
}
