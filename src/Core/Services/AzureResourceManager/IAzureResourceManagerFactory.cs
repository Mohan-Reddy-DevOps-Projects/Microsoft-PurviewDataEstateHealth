// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Purview.DataGovernance.Common;

internal interface IAzureResourceManagerFactory
{
    IAzureResourceManager Create<TAuthConfig>() where TAuthConfig : AuthConfiguration, new();
}
