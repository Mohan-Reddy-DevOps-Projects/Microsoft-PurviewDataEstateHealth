// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Text.Json;
using global::Azure;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal sealed class SparkPoolEntityAdapter : TableEntityConverter<SparkPoolModel, SparkPoolEntity>
{
    public override SparkPoolEntity ToEntity(SparkPoolModel model, string accountId)
    {
        return new SparkPoolEntity()
        {
            ETag = ETag.All,
            Id = model.Id,
            PartitionKey = accountId,
            Properties = JsonSerializer.Serialize(model.Properties),
            RowKey = model.Name,
            TenantId = model.TenantId.ToString()
        };
    }

    public override SparkPoolModel ToModel(SparkPoolEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new SparkPoolModel()
        {
            AccountId = Guid.Parse(entity.PartitionKey),
            Id = entity.Id,
            Name = entity.RowKey,
            Properties = JsonSerializer.Deserialize<SparkPoolPropertiesModel>(entity.Properties),
            TenantId = Guid.Parse(entity.TenantId),
        };
    }
}

