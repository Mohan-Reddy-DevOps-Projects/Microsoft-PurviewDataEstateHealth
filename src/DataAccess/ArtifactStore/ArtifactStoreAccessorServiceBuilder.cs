// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;

internal class ArtifactStoreAccessorServiceBuilder : IArtifactStoreAccessorServiceBuilder
{
    private readonly IDataEstateHealthRequestLogger genevaLogger;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ArtifactStoreServiceConfiguration artifactStoreServiceConfiguration;

    private readonly ArtifactStoreConfiguration artifactStoreConfiguration;

    private readonly ICertificateLoaderService certificateLoaderService;

    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactStoreAccessorServiceBuilder" /> class.
    /// </summary>
    public ArtifactStoreAccessorServiceBuilder(
        IDataEstateHealthRequestLogger genevaLogger,
        IRequestHeaderContext requestHeaderContext,
        IOptions<ArtifactStoreServiceConfiguration> artifactStoreServiceConfig,
        ICertificateLoaderService certificateLoaderService,
        IHttpContextAccessor httpContextAccessor)
    {
        this.genevaLogger = genevaLogger;
        this.requestHeaderContext = requestHeaderContext;
        this.artifactStoreServiceConfiguration = artifactStoreServiceConfig.Value;
        this.artifactStoreConfiguration = new ArtifactStoreConfiguration();
        this.certificateLoaderService = certificateLoaderService;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public IArtifactStoreAccessorService Build()
    {
        this.artifactStoreConfiguration.BaseManagementUri =
                this.artifactStoreServiceConfiguration.Endpoint;

        var certificate = this.certificateLoaderService.LoadAsync(this.artifactStoreServiceConfiguration.CertificateName, default).GetAwaiter().GetResult();
        this.artifactStoreConfiguration.Certificate = certificate ?? throw new InvalidOperationException($"ArtifactStoreAccessorServiceCollection, Cannot find artifact store certificate secret. Client certificate collection must define a certificate named '{this.artifactStoreServiceConfiguration.CertificateName}'");

        return new ArtifactStoreAccessorService(
            new ArtifactStoreEntitiesResourceAccessor(
                this.genevaLogger,
                this.httpContextAccessor,
                this.artifactStoreConfiguration),
            this.genevaLogger,
            this.requestHeaderContext);
    }
}
