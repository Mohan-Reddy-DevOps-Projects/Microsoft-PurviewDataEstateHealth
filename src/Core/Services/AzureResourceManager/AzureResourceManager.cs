// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure;
using global::Azure.Core;
using global::Azure.Identity;
using global::Azure.ResourceManager;
using global::Azure.ResourceManager.Resources;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal sealed partial class AzureResourceManager<TAuthConfig> : IAzureResourceManager where TAuthConfig : AuthConfiguration
{
    private readonly DefaultAzureCredential tokenCredential;
    private readonly ArmClient armClient;

    public AzureResourceManager(AzureCredentialFactory credentialFactory, IOptions<TAuthConfig> authConfiguration)
    {
        Uri authorityHost = new(authConfiguration.Value.Authority);
        this.tokenCredential = credentialFactory.CreateDefaultAzureCredential(authorityHost);
        this.armClient = new(this.tokenCredential);
    }

    /// <inheritdoc/>
    public SubscriptionResource GetSubscription(string resourceId)
    {
        ResourceIdentifier arn = new(resourceId);
        return this.armClient.GetSubscriptionResource(arn);
    }

    /// <inheritdoc/>
    public async Task<ResourceGroupResource> GetResourceGroup(SubscriptionResource subscription, string resourceGroupName, CancellationToken cancellationToken)
    {
        Response<ResourceGroupResource> operation = await subscription.GetResourceGroupAsync(resourceGroupName, cancellationToken);
        ResourceGroupResource resourceGroup = operation.Value;

        return resourceGroup;
    }
}
