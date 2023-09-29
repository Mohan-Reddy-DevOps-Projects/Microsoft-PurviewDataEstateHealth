// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using System;

/// <summary>
/// Represents the configuration settings needed for authentication.
/// This abstract class holds the authority, Azure region, Azure environment, and resource properties, 
/// as well as methods for getting the tenant ID and authority URI.
/// </summary>
public abstract class AuthConfiguration
{
    /// <summary>
    /// Gets or sets the URL of the authority.
    /// This is the base URL used for authentication.
    /// </summary>
    public string Authority { get; set; }

    /// <summary>
    /// Gets or sets the Azure region for the authentication.
    /// This property is used to specify the geographical region of the Azure data center.
    /// </summary>
    public string AzureRegion { get; set; }

    /// <summary>
    /// Gets or sets the Azure environment for the authentication.
    /// This property is used to specify the environment within Azure, such as public or government cloud.
    /// </summary>
    public string AzureEnvironment { get; set; }

    /// <summary>
    /// Gets or sets the resource for which the authentication is requested.
    /// This is the identifier of the target resource that the authentication is intended for.
    /// </summary>
    public string Resource { get; set; }

    /// <summary>
    /// Get the tenant id from the authority URI.
    /// The tenant ID is extracted from the segments of the authority URI.
    /// </summary>
    public string TenantId
    {
        get
        {
            if (!Uri.TryCreate(this.Authority.TrimEnd('/'), UriKind.Absolute, out Uri uri))
            {
                return null;
            }
            if (uri.Segments.Length != 2)
            {
                return null;
            }
            return uri.Segments[1];
        }
    }

    /// <summary>
    /// Get the authority URI for the specified tenant.
    /// Constructs a new URI using the authority URL and the provided tenant ID.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant for which to get the authority URI.</param>
    /// <returns>A Uri object representing the authority URI for the specified tenant.</returns>
    public Uri GetAuthorityUri(string tenantId)
    {
        UriBuilder ub = new(this.Authority)
        {
            Path = tenantId
        };
        return ub.Uri;
    }
}
