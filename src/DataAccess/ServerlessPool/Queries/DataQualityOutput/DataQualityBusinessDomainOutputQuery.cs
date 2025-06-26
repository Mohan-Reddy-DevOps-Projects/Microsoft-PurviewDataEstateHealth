// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.ServerlessPool.Queries.DataQualityOutput;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel.DataQualityOutput;
using Newtonsoft.Json;
using Records.DataQualityOutput;
using System.Collections.Generic;
using System.Data;
using static Common.QueryUtils;

[ServerlessQuery(typeof(DataQualityBusinessDomainOutputRecord))]
internal class DataQualityBusinessDomainOutputQuery : BaseQuery, IServerlessQueryRequest<DataQualityBusinessDomainOutputRecord, DataQualityBusinessDomainOutputEntity>
{
    public string QueryPath { get; set; }

    public string Query =>
        $"""

                     SELECT
                         DISTINCT
                         BusinessDomainId,
                         BusinessDomainCriticalDataElementCount,
                         Result
                     {QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.ParquetFormat)}
                     AS [result]
         """;

    public DataQualityBusinessDomainOutputRecord ParseRow(IDataRecord row)
    {
        return new DataQualityBusinessDomainOutputRecord()
        {
            BusinessDomainId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityBusinessDomainOutputRecord>(x => x.BusinessDomainId).Name]
                .ToString()),
            BusinessDomainCriticalDataElementCount = (row[GetCustomAttribute<DataColumnAttribute, DataQualityBusinessDomainOutputRecord>(x => x.BusinessDomainCriticalDataElementCount).Name] as int?),
            Result = (row[GetCustomAttribute<DataColumnAttribute, DataQualityBusinessDomainOutputRecord>(x => x.Result).Name]
                .ToString()),
        };
    }

    public IEnumerable<DataQualityBusinessDomainOutputEntity> Finalize(dynamic items)
    {
        IList<DataQualityBusinessDomainOutputEntity> entityList = new List<DataQualityBusinessDomainOutputEntity>();
        if (items == null)
        {
            return entityList;
        }

        entityList = (items as IList<DataQualityBusinessDomainOutputRecord>)!
            .Select(item => new DataQualityBusinessDomainOutputEntity()
            {
                BusinessDomainId = item.BusinessDomainId,
                BusinessDomainCriticalDataElementCount = item.BusinessDomainCriticalDataElementCount,
                Result = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Result),
            }).ToList();
        return entityList;
    }
} 