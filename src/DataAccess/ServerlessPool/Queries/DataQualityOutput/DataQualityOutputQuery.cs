// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(DataQualityDataProductOutputRecord))]
internal class DataQualityOutputQuery : BaseQuery, IServerlessQueryRequest<DataQualityDataProductOutputRecord, DataQualityDataProductOutputEntity>
{
    public string QueryPath { get; set; }

    public string Query
    {
        get => $"SELECT * {QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.ParquetFormat)} AS [result]";
    }

    public DataQualityDataProductOutputRecord ParseRow(IDataRecord row)
    {
        return new DataQualityDataProductOutputRecord()
        {
            DataProductId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.DataProductId).Name]?.ToString()),
            DataProductDisplayName = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.DataProductDisplayName).Name]?.ToString()),
            DataProductStatusDisplayName = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.DataProductStatusDisplayName).Name]?.ToString()),
            BusinessDomainId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.BusinessDomainId).Name]?.ToString()),
            DataProductOwnerIds = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.DataProductOwnerIds).Name]?.ToString()),
            Result = (row[GetCustomAttribute<DataColumnAttribute, DataQualityDataProductOutputRecord>(x => x.Result).Name]?.ToString()),
        };
    }

    public IEnumerable<DataQualityDataProductOutputEntity> Finalize(dynamic records)
    {
        IList<DataQualityDataProductOutputEntity> entityList = new List<DataQualityDataProductOutputEntity>();
        if (records == null)
        {
            return entityList;
        }

        entityList = (records as IList<DataQualityDataProductOutputRecord>)
            .Select(item => new DataQualityDataProductOutputEntity()
            {
                DataProductID = item.DataProductId,
                DataProductDisplayName = item.DataProductDisplayName,
                DataProductStatusDisplayName = item.DataProductStatusDisplayName,
                BusinessDomainId = item.BusinessDomainId,
                DataProductOwnerIds = item.DataProductOwnerIds,
                Result = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Result),
            }).ToList();
        return entityList;
    }
}
