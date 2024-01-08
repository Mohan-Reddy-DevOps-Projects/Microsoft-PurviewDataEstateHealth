// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// Component to manage a sql login and user for the workspaces.
/// </summary>
public interface IPowerBICredentialComponent
{
    /// <summary>
    /// Creates a user credential for the database  
    /// </summary>
    PowerBICredential CreateCredential(Guid accountId, string owner);

    /// <summary>
    /// Creates a user credential for the database  
    /// </summary>
    DatabaseMasterKey CreateMasterKey(string databaseName);

    /// <summary>
    /// Gets the login information for the workspace
    /// </summary>
    /// <returns></returns>
    Task<PowerBICredential> GetSynapseDatabaseLoginInfo(Guid accountId, string owner, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the login information to the key vault
    /// </summary>
    /// <returns></returns>
    Task AddOrUpdateSynapseDatabaseLoginInfo(PowerBICredential credential, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the master-key information for the workspace
    /// </summary>
    /// <returns></returns>
    Task<DatabaseMasterKey> GetSynapseDatabaseMasterKey(string databaseName, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the master-key information to the key vault
    /// </summary>
    /// <returns></returns>
    Task AddOrUpdateSynapseDatabaseMasterKey(DatabaseMasterKey credential, CancellationToken cancellationToken);
}
