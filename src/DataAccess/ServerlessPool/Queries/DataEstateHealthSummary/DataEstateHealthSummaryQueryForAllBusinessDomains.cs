// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(DataEstateHealthSummaryEntity))]
internal class DataEstateHealthSummaryQueryForAllBusinessDomains : BaseQuery, IServerlessQueryRequest<DataEstateHealthSummaryRecordForAllBusinessDomains, DataEstateHealthSummaryEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/HealthSummary/";

    public string Query
    {
        get => "SELECT TotalBusinessDomains, BusinessDomainsFilterListLink, BusinessDomainsTrendLink, LastRefreshDate," +
               "TotalCuratedDataAssetsCount, TotalCuratableDataAssetsCount, TotalNonCuratableDataAssetsCount, DataAssetsTrendLink," +
               "TotalDataProductsCount, DataProductsTrendLink," +
               "TotalOpenActionsCount, TotalCompletedActionsCount, TotalDismissedActionsCount, HealthActionsTrendLink" +
               QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
               "WITH(TotalBusinessDomains BIGINT, BusinessDomainsFilterListLink nvarchar(512),BusinessDomainsTrendLink nvarchar(512), LastRefreshDate DATE," +
               "TotalCuratedDataAssetsCount BIGINT, TotalCuratableDataAssetsCount BIGINT, TotalNonCuratableDataAssetsCount BIGINT, DataAssetsTrendLink nvarchar(512)," +
               "TotalDataProductsCount BIGINT, DataProductsTrendLink nvarchar(512)," +
               "TotalOpenActionsCount BIGINT, TotalCompletedActionsCount BIGINT, TotalDismissedActionsCount BIGINT, HealthActionsTrendLink nvarchar(512)" +
               QueryConstants.ServerlessQuery.AsRows;
    }

    public DataEstateHealthSummaryRecordForAllBusinessDomains ParseRow(IDataRecord row)
    {
        return new DataEstateHealthSummaryRecordForAllBusinessDomains()
        {
            TotalBusinessDomains =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalBusinessDomains).Name]?.ToString()).AsInt(),
            BusinessDomainsFilterListLink =
                row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.BusinessDomainsFilterListLink).Name]?.ToString(),
            BusinessDomainsTrendLink =
                 row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.BusinessDomainsTrendLink).Name]?.ToString(),
            LastRefreshDate =
                 (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.LastRefreshDate).Name]?.ToString()).AsDateTime(),
            TotalCuratedDataAssetsCount =
                 (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalCuratedDataAssetsCount).Name]?.ToString()).AsInt(),
            TotalCuratableDataAssetsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalCuratableDataAssetsCount).Name]?.ToString()).AsInt(),
            TotalNonCuratableDataAssetsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalNonCuratableDataAssetsCount).Name]?.ToString()).AsInt(),
            DataAssetsTrendLink =
                 row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.DataAssetsTrendLink).Name]?.ToString(),
            TotalDataProductsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalDataProductsCount).Name]?.ToString()).AsInt(),
            DataProductsTrendLink =
                row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.DataProductsTrendLink).Name]?.ToString(),
            TotalOpenActionsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalOpenActionsCount).Name]?.ToString()).AsInt(),
            TotalCompletedActionsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalCompletedActionsCount).Name]?.ToString()).AsInt(),
            TotalDismissedActionsCount =
                (row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.TotalDismissedActionsCount).Name]?.ToString()).AsInt(),
            HealthActionsTrendLink =
                row[GetCustomAttribute<DataColumnAttribute, DataEstateHealthSummaryRecord>(x => x.HealthActionsTrendLink).Name]?.ToString()
        };
    }

    public IEnumerable<DataEstateHealthSummaryEntity> Finalize(dynamic records)
    {
        IList<DataEstateHealthSummaryEntity> entityList = new List<DataEstateHealthSummaryEntity>();
        if (records == null)
        {
            return entityList;
        }

        foreach (DataEstateHealthSummaryRecordForAllBusinessDomains record in records)
        {
            entityList.Add(new DataEstateHealthSummaryEntity()
            {
                BusinessDomainsSummaryEntity = new BusinessDomainsSummaryEntity()
                {
                    TotalBusinessDomainsCount = record.TotalBusinessDomains,
                    BusinessDomainsFilterListLink = record.BusinessDomainsFilterListLink,
                    BusinessDomainsLastRefreshDate = record.LastRefreshDate,
                    BusinessDomainsTrendLink = record.BusinessDomainsTrendLink,
                },
                HealthActionsSummaryEntity = new HealthActionsSummaryEntity()
                {
                    HealthActionsLastRefreshDate = record.LastRefreshDate,
                    HealthActionsTrendLink = record.HealthActionsTrendLink,
                    TotalCompletedActionsCount = record.TotalCompletedActionsCount,
                    TotalDismissedActionsCount = record.TotalDismissedActionsCount,
                    TotalOpenActionsCount = record.TotalOpenActionsCount,
                },
                DataAssetsSummaryEntity = new DataAssetsSummaryEntity()
                {
                    DataAssetsLastRefreshDate = record.LastRefreshDate,
                    TotalCuratableDataAssetsCount = record.TotalCuratableDataAssetsCount,
                    TotalCuratedDataAssetsCount = record.TotalCuratedDataAssetsCount,
                    TotalNonCuratableDataAssetsCount = record.TotalNonCuratableDataAssetsCount,
                    DataAssetsTrendLink = record.DataAssetsTrendLink,
                },
                DataProductsSummaryEntity = new DataProductsSummaryEntity()
                {
                    DataProductsLastRefreshDate = record.LastRefreshDate,
                    DataProductsTrendLink = record.DataProductsTrendLink,
                    TotalDataProductsCount = record.TotalDataProductsCount,
                }
            });
        }
        return entityList;
    }
}
