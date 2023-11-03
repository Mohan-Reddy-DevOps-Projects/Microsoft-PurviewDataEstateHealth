// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

internal interface IAzureResourceManagerFactory
{
    IAzureResourceManager Create<TAuthConfig>() where TAuthConfig : AuthConfiguration, new();
}
