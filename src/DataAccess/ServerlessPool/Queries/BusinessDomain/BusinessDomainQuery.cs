// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(BusinessDomainEntity))]
internal class BusinessDomainQuery : BaseQuery, IServerlessQueryRequest<BusinessDomainRecord, BusinessDomainEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/BusinessDomainSchema/";

    public string Query
    {
        get => "SELECT BusinessDomainId, BusinessDomainDisplayName" +
                           QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
                           "WITH(BusinessDomainId nvarchar(36), BusinessDomainDisplayName nvarchar(max))" +
                           QueryConstants.ServerlessQuery.AsRows + this.FilterClause;
    }

    public BusinessDomainRecord ParseRow(IDataRecord row)
    {
        return new BusinessDomainRecord()
        {
            BusinessDomainId =
                Guid.Parse(row[GetCustomAttribute<DataColumnAttribute, BusinessDomainRecord>(x => x.BusinessDomainId).Name].ToString()),
            BusinessDomainDisplayName =
                row[GetCustomAttribute<DataColumnAttribute, BusinessDomainRecord>(x => x.BusinessDomainDisplayName).Name]?.ToString(),
        };
    }

    public IEnumerable<BusinessDomainEntity> Finalize(dynamic records)
    {
        IList<BusinessDomainEntity> entityList = new List<BusinessDomainEntity>();
        if (records == null)
        {
            return entityList;
        }

        foreach (BusinessDomainRecord record in records)
        {
            entityList.Add(new BusinessDomainEntity()
            {
                BusinessDomainId = record.BusinessDomainId,
                BusinessDomainName = record.BusinessDomainDisplayName,
            });
        }

        return entityList;
    }
}
