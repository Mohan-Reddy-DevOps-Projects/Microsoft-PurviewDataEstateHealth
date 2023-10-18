// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;

/// <summary>
/// Partner details per operation.
/// </summary>
public interface IPartnerDetailsPerOperation : IPartnerDetails
{
    /// <summary>
    /// Gets the operation type.
    /// </summary>
    OperationType OperationType { get; }

    /// <summary>
    /// Gets the http verb.
    /// </summary>
    string HttpVerb { get; }

    /// <summary>
    /// Gets the operation timeout seconds.
    /// </summary>
    int TimeoutSeconds { get; }

    /// <summary>
    /// Gets the timeout in seconds for polling.
    /// </summary>
    int PollingTimeoutSeconds { get; }

    /// <summary>
    /// Gets the polling interval in seconds.
    /// </summary>
    int PollingIntervalSeconds { get; }
}
