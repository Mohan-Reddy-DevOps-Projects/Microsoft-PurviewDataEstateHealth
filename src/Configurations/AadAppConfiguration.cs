// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Represents the configuration settings needed for Azure Active Directory (AAD) application authentication.
/// This class inherits from the AuthConfiguration class and includes additional properties specific 
/// to AAD application authentication, such as ClientUri, CertificateName, ClientId, and 
/// RenewBeforeExpiryInMinutes.
/// </summary>
public abstract class AadAppConfiguration : AuthConfiguration
{
    /// <summary>
    /// Gets or sets the client URI used for AAD application authentication.
    /// This URI is used to uniquely identify the client application within AAD.
    /// </summary>
    public string ClientUri { get; set; }

    /// <summary>
    /// Gets or sets the name of the certificate used for AAD application authentication.
    /// </summary>
    public string CertificateName { get; set; }

    /// <summary>
    /// Gets or sets the client ID used for AAD application authentication.
    /// This is the identifier assigned to the application upon registration in AAD.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the time in minutes before expiry when the token should be renewed.
    /// This property is used to ensure that the authentication token is renewed before it expires.
    /// </summary>
    public int RenewBeforeExpiryInMinutes { get; set; }
}

