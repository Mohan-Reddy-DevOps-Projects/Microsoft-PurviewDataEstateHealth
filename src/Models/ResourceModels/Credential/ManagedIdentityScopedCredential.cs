// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// A scoped credential for SQL data sources using MI.
/// </summary>
public class ManagedIdentityScopedCredential
{
    /// <summary>
    /// Instantiates an instance of ManagedIdentityScopedCredential.
    /// </summary>
    public ManagedIdentityScopedCredential(string name)
    {
        this.Name = name;
        this.Identity = "Managed Identity";
    }

    /// <summary>
    /// The name of the credential.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The identity to use.
    /// </summary>
    public string Identity { get; }
}
