// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using System.Security.Cryptography.X509Certificates;
using Microsoft.Purview.ArtifactStoreClient;

internal class ArtifactStoreConfiguration : IArtifactStoreConfiguration
{
    /// <summary>
    /// The client Id of the AAD application used to access the service
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// Azure AD authority endpoint including the tenant id
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// The Azure region for the local login authority
    /// </summary>
    public string AzureRegion { get; set; }

    /// <summary>
    /// Artifact store AAD resource id
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// Artifact store Url
    /// </summary>
    public string BaseManagementUri { get; set; }

    /// <summary>
    /// Certificate subject name - unused
    /// </summary>
    public string SubjectName { get; set; }

    /// <summary>
    /// TenantId - unused - the tenant Id is incorporated in the Authority
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// The certificate used to authenticate with Artifact Store
    /// </summary>
    public X509Certificate2 Certificate { get; set; }

    /// <summary>
    /// The authorization method used to authenticate with Artifact Store
    /// </summary>
    public AuthorizationMethod AuthorizationMethod { get => AuthorizationMethod.ClientCertificate; }
}
