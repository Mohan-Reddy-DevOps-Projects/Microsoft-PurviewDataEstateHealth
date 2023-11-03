// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Security;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Context for database related requests.
/// </summary>
public interface IDatabaseRequest
{
    /// <summary>
    /// The database name.
    /// </summary>
    string DatabaseName { get; }

    /// <summary>
    /// The data source location.
    /// </summary>
    string DataSourceLocation { get; }

    /// <summary>
    /// The schema name.
    /// </summary>
    string SchemaName { get; }

    /// <summary>
    /// The master key for the database.
    /// </summary>
    SecureString MasterKey { get; }

    /// <summary>
    /// The login name.
    /// </summary>
    string LoginName { get; }

    /// <summary>
    /// The login password.
    /// </summary>
    SecureString LoginPassword { get; }

    /// <summary>
    /// The user name.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// The scoped credential used to authenticate with data sources.
    /// </summary>
    ManagedIdentityScopedCredential ScopedCredential { get; }
}
