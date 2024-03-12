// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal sealed class MDQJobEntityAdapter : TableEntityConverter<MDQJobModel, MDQFailedJobEntity>
{
    public static string DefaultPartitionKey = Guid.Empty.ToString();

    public override MDQFailedJobEntity ToEntity(MDQJobModel model, string _)
    {
        return new MDQFailedJobEntity()
        {
            ETag = ETag.All,
            PartitionKey = DefaultPartitionKey,
            RowKey = model.DQJobId.ToString(),
            TenantId = model.TenantId.ToString(),
            AccountId = model.AccountId.ToString(),
            JobStatus = model.JobStatus,
            DQJobId = model.DQJobId.ToString(),
        };
    }

    public override MDQJobModel ToModel(MDQFailedJobEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new MDQJobModel()
        {
            TenantId = Guid.Parse(entity.TenantId),
            AccountId = Guid.Parse(entity.AccountId),
            DQJobId = Guid.Parse(entity.DQJobId),
            JobStatus = entity.JobStatus,
            Timestamp = entity.Timestamp,
        };
    }
}

