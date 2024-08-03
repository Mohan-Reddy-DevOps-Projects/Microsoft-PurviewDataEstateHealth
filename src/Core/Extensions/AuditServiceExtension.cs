// <copyright file="AuditServiceExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Extensions;

using global::Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.AuditAPI;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common.AAD;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using System.Security.Cryptography.X509Certificates;

public static class AuditServiceExtensions
{
    public static void SetupAuditService(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
        {

            services.Configure<AuditServiceConfiguration>(configuration.GetSection(AuditServiceConfiguration.ConfigSectionName));

            services.AddSingleton<AuditAadAppConfiguration>();

            services.AddSingleton(sp =>
            {
                var logger = sp.GetService<IDataEstateHealthRequestLogger>();
                var environmentConfig = sp.GetService<IOptionsMonitor<EnvironmentConfiguration>>();
                var auditServiceConfiguration = sp.GetService<IOptionsMonitor<AuditServiceConfiguration>>();
                var auditAadAppConfiguration = sp.GetService<AuditAadAppConfiguration>();

                var auditClientFactory = new AuditClientFactory(
                    auditAadAppConfiguration,
                    new AzureLocation(environmentConfig.CurrentValue.Location),
                    auditServiceConfiguration.CurrentValue.AuditEnvironment, logger);

                return auditClientFactory;
            });
        }
    }

    internal class AuditAadAppConfiguration : AadAppTokenProviderConfiguration
    {
        private readonly ICertificateLoaderService certificateLoaderService;
        private readonly string certificateName;

        public AuditAadAppConfiguration(
            IOptionsMonitor<AuditServiceConfiguration> auditServiceConfiguration,
            IOptionsMonitor<EnvironmentConfiguration> environmentConfig,
            ICertificateLoaderService certManager)
        {
            this.certificateLoaderService = certManager;

            this.Authority = auditServiceConfiguration.CurrentValue.AADAuth.Authority;
            this.ClientId = auditServiceConfiguration.CurrentValue.AADAuth.ClientId;
            this.certificateName = auditServiceConfiguration.CurrentValue.AADAuth.CertificateName;

            this.RenewBeforeExpiryInMinutes = 2;
            this.AzureRegion = environmentConfig.CurrentValue.Location;
        }

        public async override Task<X509Certificate2> GetCertificateAsync()
        {
            var certificate = await this.certificateLoaderService.LoadAsync(this.certificateName, CancellationToken.None);
            return certificate;
        }
    }

}

