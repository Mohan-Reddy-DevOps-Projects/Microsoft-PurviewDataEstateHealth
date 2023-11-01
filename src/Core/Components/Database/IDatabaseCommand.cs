// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal interface IDatabaseCommand
{
    Task AddDatabaseAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task AddMasterKeyAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task AddScopedCredentialAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task AddLoginAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task AddUserAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task CreateSchemaAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task GrantUserToSchemaAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task GrantCredentialToUserAsync(IDatabaseRequest request, CancellationToken cancellationToken);

    Task ExecuteScriptAsync(IDatabaseRequest request, CancellationToken cancellationToken);
}
