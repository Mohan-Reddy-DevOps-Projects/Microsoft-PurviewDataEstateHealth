// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Common;

internal sealed class AzureResourceManagerFactory : IAzureResourceManagerFactory
{
    private readonly IServiceProvider serviceProvider;

    public AzureResourceManagerFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IAzureResourceManager Create<TAuthConfig>() where TAuthConfig : AuthConfiguration, new()
    {
        return ActivatorUtilities.CreateInstance<AzureResourceManager<TAuthConfig>>(this.serviceProvider);
    }
}
