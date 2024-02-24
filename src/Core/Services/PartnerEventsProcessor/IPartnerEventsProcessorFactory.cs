// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal interface IPartnerEventsProcessorFactory
{
    IPartnerEventsProcessor Build(EventSourceType eventSourceType);
}
