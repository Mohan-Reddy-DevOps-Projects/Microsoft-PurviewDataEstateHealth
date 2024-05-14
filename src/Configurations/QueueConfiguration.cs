// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configuration for queue
/// </summary>
public class QueueConfiguration
{
    /// <summary>
    /// Queue service uri
    /// </summary>
    public string QueueServiceUri { get; set; }

    /// <summary>
    /// Queue name
    /// </summary>
    public string QueueName { get; set; }
}

/// <summary>
/// Configuration for triggered schedule queue
/// </summary>
public class TriggeredScheduleQueueConfiguration : QueueConfiguration
{
}
