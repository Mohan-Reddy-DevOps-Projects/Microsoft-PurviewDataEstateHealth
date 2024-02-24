// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Purview.DataGovernance.Common;

public interface IAzureResourceManagerFactory
{
    IAzureResourceManager Create<TAuthConfig>() where TAuthConfig : AuthConfiguration, new();
}
