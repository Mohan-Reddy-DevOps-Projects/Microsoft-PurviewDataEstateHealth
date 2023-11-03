// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;

internal sealed class ProcessingStorageRepository : StorageAccountRepository<ProcessingStorageModel, ProcessingStorageEntity>, IStorageAccountRepository<ProcessingStorageModel>
{
    private static readonly ProcessingStorageEntityAdapter converter = new();

    public ProcessingStorageRepository(
        ITableStorageClient tableStorageClient,
        IOptions<AccountStorageTableConfiguration> tableConfiguration) : base(tableStorageClient, converter, tableConfiguration)
    {
    }
}
