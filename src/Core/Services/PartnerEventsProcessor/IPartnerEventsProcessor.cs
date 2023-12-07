// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

internal interface IPartnerEventsProcessor
{
    Task StartAsync(int maxProcessingTimeInSeconds = 10, int maxTimeoutInSeconds = 120);

    Task StopAsync();

    Task CommitAsync(IDictionary<Guid, string> processingStoresCache = null);
}
