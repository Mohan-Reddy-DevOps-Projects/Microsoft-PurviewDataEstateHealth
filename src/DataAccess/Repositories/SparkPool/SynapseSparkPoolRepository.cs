// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;

internal sealed class SynapseSparkPoolRepository : SparkPoolRepository<SparkPoolModel, SparkPoolEntity>, ISparkPoolRepository<SparkPoolModel>
{
    private static readonly SparkPoolEntityAdapter converter = new();

    public SynapseSparkPoolRepository(
        ITableStorageClient<SparkPoolTableConfiguration> tableStorageClient,
        IOptions<SparkPoolTableConfiguration> tableConfiguration) : base(tableStorageClient, converter, tableConfiguration)
    {
    }
}
