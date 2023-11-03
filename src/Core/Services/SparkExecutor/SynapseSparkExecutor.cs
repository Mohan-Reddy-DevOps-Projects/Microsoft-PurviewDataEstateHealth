// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

internal class SynapseSparkExecutor : ISynapseSparkExecutor
{
    private readonly IAzureResourceManager azureResourceManager;
    private readonly SynapseSparkConfiguration synapseSparkConfiguration;

    public SynapseSparkExecutor(IAzureResourceManagerFactory azureResourceManagerFactory, IOptions<SynapseSparkConfiguration> synapseAuthConfiguration)
    {
        this.azureResourceManager = azureResourceManagerFactory.Create<ProcessingStorageAuthConfiguration>();
        this.synapseSparkConfiguration = synapseAuthConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task CreateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        // TODO: create a unique name
        string sparkPoolName = "dghdogfoodsynapse-sparkpool";
        // TODO: ensure that it does not already exist
        // TODO: cannot include the library package
        await this.azureResourceManager.CreateSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, this.synapseSparkConfiguration.AzureRegion, cancellationToken);
    }
}
