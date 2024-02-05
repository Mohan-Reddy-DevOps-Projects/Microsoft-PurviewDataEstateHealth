// <copyright file="AzureCredentialManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret
{
    using global::Azure.Core;
    using global::Azure.Identity;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Access an Azure credential instance
    /// </summary>
    public class DHCosmosDBContextAzureCredentialManager
    {
        public DHCosmosDBContextAzureCredentialManager(IWebHostEnvironment env)
        {
            var IsDevelopment = env.IsDevelopment();
            this.Credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeManagedIdentityCredential = IsDevelopment, });
        }

        public TokenCredential Credential { get; }
    }
}
