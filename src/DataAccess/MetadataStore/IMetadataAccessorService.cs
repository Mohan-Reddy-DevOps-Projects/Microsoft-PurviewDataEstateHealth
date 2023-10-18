// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.MetadataStore;

using System.Threading.Tasks;

internal interface IMetadataAccessorService
{
    Task<string> GetManagedIdentityTokensAsync(
            string accountId,
            CancellationToken cancellationToken);
}
