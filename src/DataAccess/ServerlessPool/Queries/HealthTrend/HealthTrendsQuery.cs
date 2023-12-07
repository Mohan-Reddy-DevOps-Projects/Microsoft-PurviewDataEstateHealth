// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.ServerlessPool.Records.HealthTrend;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthTrendRecord))]
internal class HealthTrendsQuery : BaseQuery, IServerlessQueryRequest<HealthTrendRecord, HealthTrendEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/BusinessDomainTrendsById/";

    public string Query
    {
        get => "SELECT " + this.SelectClause + ", LastRefreshedAt, BusinessDomainId" +
                           QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
                           "WITH(" + this.SelectClause + " BIGINT, LastRefreshedAt DateTime2, BusinessDomainId uniqueidentifier)" +
                           QueryConstants.ServerlessQuery.AsRows +
                            this.FilterClause;
    }

    public HealthTrendRecord ParseRow(IDataRecord row)
    {
        return new HealthTrendRecord()
        {
            HealthTrendDataValue = row[this.SelectClause].ToString().AsInt(),
            LastRefreshedAt =
                 (row[GetCustomAttribute<DataColumnAttribute, HealthTrendRecord>(x => x.LastRefreshedAt).Name]?.ToString()).AsDateTime(),
        };
    }

    private TrendKind SwitchOnColumnName(string columnName)
    {
        switch (columnName)
        {
            case "TotalOpenActionsCount":
                return TrendKind.OpenActions;
            case "BusinessDomainCount":
                return TrendKind.BusinessDomainCount;
            case "DataProductCount":
                return TrendKind.DataProductCount;
            case "AssetCount":
                return TrendKind.DataAssetCount;
            default:
                throw new ServiceError(ErrorCategory.InputError, ErrorCode.HealthTrends_InvalidColumnName.Code, $"Invalid columnName: {columnName}, not a TrendKind").ToException();
        }
    }

    public IEnumerable<HealthTrendEntity> Finalize(dynamic records)
    {
        IList<HealthTrendEntity> entityList = new List<HealthTrendEntity>();
        if (records == null)
        {
            return entityList;
        }

        List<TrendValue> trendValuesList = new();
        foreach (HealthTrendRecord record in records)
        {
            trendValuesList.Add(new TrendValue()
            {
                CaptureDate = record.LastRefreshedAt,
                Value = record.HealthTrendDataValue.ToString(),
            });
        }

        TrendKind trendKind = this.SwitchOnColumnName(this.SelectClause);

        entityList.Add(new HealthTrendEntity()
        {
            Kind = trendKind,
            Description = $"Trends for {trendKind} for all business domains",
            TrendValuesList = trendValuesList,
        });

        return entityList;
    }
}
