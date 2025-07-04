﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthActionsRecord))]
internal class HealthActionsQuery : BaseQuery, IServerlessQueryRequest<HealthActionsRecord, HealthActionEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/ActionCenter/";

    public string Query
    {
        get => "SELECT RowId, ActionId, DisplayName, Description, HealthControlState, HealthControlName," +
               "BusinessDomainId, TargetType, TargetId, TargetName, OwnerContactId, OwnerContactDisplayName," +
               "ActionStatus, LastRefreshedAt, CreatedAt" +
               QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
               "WITH (RowId nvarchar(64), ActionId nvarchar(128), DisplayName nvarchar(128), Description nvarchar(1024), HealthControlState nvarchar(32), HealthControlName nvarchar(64)," +
               "BusinessDomainId uniqueidentifier, TargetType nvarchar(128), TargetId nvarchar(64), TargetName nvarchar(64), OwnerContactId nvarchar(64), OwnerContactDisplayName nvarchar(128), " +
               "ActionStatus nvarchar(32), LastRefreshedAt DateTime2, CreatedAt DateTime2)" +
               QueryConstants.ServerlessQuery.AsRows + this.FilterClause;
    }

    public HealthActionsRecord ParseRow(IDataRecord row)
    {
        var ownerContactId = row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.OwnerContactId).Name]?.ToString();

        return new HealthActionsRecord()
        {
            BusinessDomainId =
                (row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.BusinessDomainId).Name]?.ToString()).AsGuid(),
            ActionId =
                row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.RowId).Name].ToString().AsGuid(),
            TargetId =
                row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.TargetId).Name].ToString().AsGuid(),
            TargetName =
                row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.TargetName).Name].ToString(),
            OwnerContactId = string.IsNullOrEmpty(ownerContactId) ? Guid.Empty : ownerContactId.AsGuid(),
            OwnerContactDisplayName =
                row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.OwnerContactDisplayName).Name]?.ToString(),
            DisplayName =
                row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.DisplayName).Name]?.ToString(),
            Description =
                 row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.Description).Name]?.ToString(),
            TargetType =
                 row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.TargetType).Name]?.ToString(),
            HealthControlName =
                 row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.HealthControlName).Name]?.ToString(),
            HealthControlState =
                 row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.HealthControlState).Name].ToString(),
            ActionStatus =
                 row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.ActionStatus).Name]?.ToString(),
            LastRefreshedAt =
                 (row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.LastRefreshedAt).Name]?.ToString()).AsDateTime(),
            CreatedAt =
                 (row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.CreatedAt).Name]?.ToString()).AsDateTime(),
        };
    }

    public IEnumerable<HealthActionEntity> Finalize(dynamic records)
    {
        IList<HealthActionEntity> entityList = new List<HealthActionEntity>();
        if (records == null)
        {
            return entityList;
        }

        entityList = (records as IList<HealthActionsRecord>)
            .GroupBy(rec => rec.ActionId)
            .Select(group => new HealthActionEntity()
            {
                Id = group.Key,
                Name = group.Select(rec => rec.DisplayName).FirstOrDefault(),
                Description = group.Select(rec => rec.Description).FirstOrDefault(),
                OwnerContact = new Models.OwnerContact
                {
                    ObjectId = group.Select(rec => rec.OwnerContactId).FirstOrDefault(),
                    DisplayName = group.Select(rec => rec.OwnerContactDisplayName).FirstOrDefault(),
                },
                HealthControlName = group.Select(rec => rec.HealthControlName).FirstOrDefault(),
                HealthControlState = group.Select(rec => Enum.Parse<HealthControlState>(rec.HealthControlState)).FirstOrDefault(),
                CreatedAt = group.Select(rec => rec.CreatedAt).FirstOrDefault(),
                LastRefreshedAt = group.Select(rec => rec.LastRefreshedAt).FirstOrDefault(),
                TargetDetailsList = group.Select(rec => new TargetDetails
                {
                    OwnerContact = new Models.OwnerContact
                    {
                        ObjectId = group.Select(rec => rec.OwnerContactId).FirstOrDefault(),
                        DisplayName = group.Select(rec => rec.OwnerContactDisplayName).FirstOrDefault(),
                    },
                    TargetKind = Enum.Parse<TargetKind>(rec.TargetType),
                    TargetId = rec.TargetId,
                    TargetName = rec.TargetName
                }).ToList()
            }).ToList();

        return entityList;
    }
}
