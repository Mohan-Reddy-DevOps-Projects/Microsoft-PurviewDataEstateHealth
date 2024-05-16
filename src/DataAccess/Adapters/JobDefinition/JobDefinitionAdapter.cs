// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using global::Azure;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal sealed class JobDefinitionAdapter : TableEntityConverter<JobDefinitionModel, JobDefinitionEntity>
{
    public static string DefaultPartitionKey = Guid.Empty.ToString();

    public override JobDefinitionEntity ToEntity(JobDefinitionModel model, string _)
    {
        return new JobDefinitionEntity()
        {
            ETag = ETag.All,
            PartitionKey = DefaultPartitionKey,
            JobId = model.JobId,
            Callback = model.Callback,
            JobPartition = model.JobPartition,
            LastExecutionStatus = model.LastExecutionStatus,
        };
    }

    public override JobDefinitionModel ToModel(JobDefinitionEntity entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new JobDefinitionModel()
        {
            JobId = entity.JobId,
            JobPartition = entity.JobPartition,
            Callback = entity.Callback,
            LastExecutionStatus = entity.LastExecutionStatus,
        };
    }
}

