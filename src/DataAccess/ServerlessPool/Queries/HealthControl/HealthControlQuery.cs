// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthControlRecord))]
internal class HealthControlQuery : BaseQuery, IServerlessQueryRequest<HealthControlRecord, DataGovernanceHealthControlEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/HealthScores/";

    public string Query
    {
        get => "SELECT ActualValue, LastRefreshedAt " +
            QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
            "WITH (ActualValue float, LastRefreshedAt DateTime2)" +
            QueryConstants.ServerlessQuery.AsRows;
    }

    public HealthControlRecord ParseRow(IDataRecord row)
    {
        return new HealthControlRecord()
        {
            ActualValue =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.ActualValue).Name].ToString()),
            LastRefreshedAt =
                Convert.ToDateTime(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.LastRefreshedAt).Name].ToString())
        };
    }

    public IEnumerable<DataGovernanceHealthControlEntity> Finalize(dynamic records)
    {
        IList<DataGovernanceHealthControlEntity> entityList = new List<DataGovernanceHealthControlEntity>();
        if (records == null)
        {
            return entityList;
        }

        foreach (HealthControlRecord record in records)
        {
            //Add parent data governance
            entityList.Add(new DataGovernanceHealthControlEntity()
            {
                CurrentScore = record.ActualValue,
                LastRefreshedAt = record.LastRefreshedAt
            });
        }

        return entityList;
    }
}
