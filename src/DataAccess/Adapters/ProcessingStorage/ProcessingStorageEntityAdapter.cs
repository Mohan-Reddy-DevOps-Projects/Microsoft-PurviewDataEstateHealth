// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Text.Json;
using global::Azure;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal sealed class ProcessingStorageEntityAdapter : TableEntityConverter<ProcessingStorageModel, ProcessingStorageEntity>
{
    public override ProcessingStorageEntity ToEntity(ProcessingStorageModel model, string accountId)
    {
        return new ProcessingStorageEntity()
        {
            ETag = ETag.All,
            Id = model.Id,
            PartitionKey = accountId,
            Properties = JsonSerializer.Serialize(model.Properties),
            RowKey = model.Name,
            TenantId = model.TenantId.ToString(),
            CatalogId = model.CatalogId.ToString(),
        };
    }

    public override ProcessingStorageModel ToModel(ProcessingStorageEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new ProcessingStorageModel()
        {
            AccountId = Guid.Parse(entity.PartitionKey),
            Id = entity.Id,
            Name = entity.RowKey,
            Properties = JsonSerializer.Deserialize<ProcessingStoragePropertiesModel>(entity.Properties),
            TenantId = Guid.Parse(entity.TenantId),
            CatalogId = Guid.Parse(entity.CatalogId)
        };
    }
}

