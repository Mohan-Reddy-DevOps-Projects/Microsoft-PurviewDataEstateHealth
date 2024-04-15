// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using System.Security;
using System.Text.Json;

/// <inheritdoc/>
public class DatabaseRequest : IDatabaseRequest
{
    /// <inheritdoc/>
    public string DatabaseName { get; init; }

    /// <inheritdoc/>
    public string DataSourceLocation { get; init; }

    /// <inheritdoc/>
    public string SchemaName { get; init; }

    /// <inheritdoc/>
    public SecureString MasterKey { get; init; }

    /// <inheritdoc/>
    public string LoginName { get; init; }

    /// <inheritdoc/>
    public SecureString LoginPassword { get; init; }

    /// <inheritdoc/>
    public string UserName { get; init; }

    /// <inheritdoc/>
    public ManagedIdentityScopedCredential ScopedCredential { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
