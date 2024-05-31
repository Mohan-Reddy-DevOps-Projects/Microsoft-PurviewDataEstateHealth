// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using System.Collections.Generic;
using System.Data;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(DataQualityScoreRecord))]
internal class DataQualityScoreQuery : BaseQuery, IServerlessQueryRequest<DataQualityScoreRecord, DataQualityScoreEntity>
{
    public Guid AccountId { get; set; }

    public string QueryPath => string.Empty;

    public string Query
    {
        get => this.QueryByDimension ? @"
SELECT
    Score, BusinessDomainSourceId AS BusinessDomainId, DataProductSourceId AS DataProductId, DataAssetSourceId AS DataAssetId, DQJobSourceId AS DQJobId, ExecutionTime, DataProductStatusDisplayName AS DataProductStatus, DataProductOwnerIds, QualityDimension
FROM
    (SELECT
        *
    FROM (
        SELECT
            Score,
            ROW_NUMBER() OVER(PARTITION BY BusinessDomainId, DataProductId, DataAssetId, QualityDimension ORDER BY ExecutionTime DESC) as row_num,
            BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId, ExecutionTime, QualityDimension
        FROM
            (
                    SELECT
                        AVG(DQOverallProfileQualityScore) as Score,
                        BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId, MAX(RuleScanCompletionDatetime) AS ExecutionTime, QualityDimension
                    " + this.OpenFactDataQuality + @"AS [result]
                        JOIN (SELECT DQRuleTypeId, QualityDimension " + this.OpenRuleType + @"AS [result]) RuleType ON [result].DQRuleTypeId = RuleType.DQRuleTypeId
                        JOIN (SELECT JobTypeId, JobTypeDisplayName " + this.OpenJobType + @"AS [result]) JobType ON [result].JobTypeId = JobType.JobTypeId
                    WHERE JobTypeDisplayName = 'DQ'
                    GROUP BY BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId, QualityDimension
            ) TMP1
        ) TMP2 WHERE row_num = 1
    ) DQFact
JOIN (SELECT BusinessDomainId, BusinessDomainSourceId " + this.OpenBusinessDomain + @" AS [result]) BD ON DQFact.BusinessDomainId = BD.BusinessDomainId
JOIN (SELECT DataProductId, DataProductSourceId " + this.OpenDataProduct + @" AS [result]) DP ON DQFact.DataProductId = DP.DataProductId
JOIN (SELECT DataAssetId, DataAssetSourceId " + this.OpenDataAsset + @" AS [result]) DA ON DQFact.DataAssetId = DA.DataAssetId
JOIN (SELECT DataProductId, DataProductStatusID " + this.OpenDataProductDetail + @" AS [result]) DPDetail ON DP.DataProductSourceId = DPDetail.DataProductId
JOIN (SELECT DataProductStatusID, DataProductStatusDisplayName " + this.OpenDataProductStatus + @" AS [result]) DPStatus ON DPDetail.DataProductStatusID = DPStatus.DataProductStatusID
JOIN (SELECT DataProductId, STRING_AGG(DataProductOwnerId, ',') AS DataProductOwnerIds " + this.OpenDataProductOwners + @" AS [result] GROUP BY DataProductId) DPOwner ON DP.DataProductSourceId = DPOwner.DataProductId
" : @"
SELECT
    Score, BusinessDomainSourceId AS BusinessDomainId, DataProductSourceId AS DataProductId, DataAssetSourceId AS DataAssetId, DQJobSourceId AS DQJobId, ExecutionTime, DataProductStatusDisplayName AS DataProductStatus, DataProductOwnerIds
FROM
    (SELECT
        *
    FROM (
        SELECT
            Score,
            ROW_NUMBER() OVER(PARTITION BY BusinessDomainId, DataProductId, DataAssetId ORDER BY ExecutionTime DESC) as row_num,
            BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId, ExecutionTime
        FROM
            (
                    SELECT
                        AVG(DQOverallProfileQualityScore) as Score,
                        BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId, MAX(RuleScanCompletionDatetime) AS ExecutionTime
                    " + this.OpenFactDataQuality + @"AS [result]
                        JOIN (SELECT DQRuleTypeId, QualityDimension " + this.OpenRuleType + @"AS [result]) RuleType ON [result].DQRuleTypeId = RuleType.DQRuleTypeId
                        JOIN (SELECT JobTypeId, JobTypeDisplayName " + this.OpenJobType + @"AS [result]) JobType ON [result].JobTypeId = JobType.JobTypeId
                    WHERE JobTypeDisplayName = 'DQ'
                    GROUP BY BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId
            ) TMP1
        ) TMP2 WHERE row_num = 1
    ) DQFact
JOIN (SELECT BusinessDomainId, BusinessDomainSourceId " + this.OpenBusinessDomain + @" AS [result]) BD ON DQFact.BusinessDomainId = BD.BusinessDomainId
JOIN (SELECT DataProductId, DataProductSourceId " + this.OpenDataProduct + @" AS [result]) DP ON DQFact.DataProductId = DP.DataProductId
JOIN (SELECT DataAssetId, DataAssetSourceId " + this.OpenDataAsset + @" AS [result]) DA ON DQFact.DataAssetId = DA.DataAssetId
JOIN (SELECT DataProductId, DataProductStatusID " + this.OpenDataProductDetail + @" AS [result]) DPDetail ON DP.DataProductSourceId = DPDetail.DataProductId
JOIN (SELECT DataProductStatusID, DataProductStatusDisplayName " + this.OpenDataProductStatus + @" AS [result]) DPStatus ON DPDetail.DataProductStatusID = DPStatus.DataProductStatusID
JOIN (SELECT DataProductId, STRING_AGG(DataProductOwnerId, ',') AS DataProductOwnerIds " + this.OpenDataProductOwners + @" AS [result] GROUP BY DataProductId) DPOwner ON DP.DataProductSourceId = DPOwner.DataProductId
";
    }

    public bool QueryByDimension = false;

    private string DomainModelDbName => $"{this.AccountId}.DomainModel";
    private string DimensionalModelModelDbName => $"{this.AccountId}.DimensionalModel";

    private string OpenFactDataQuality => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "FactDataQuality");
    private string OpenBusinessDomain => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "DimBusinessDomain");
    private string OpenDataProduct => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "DimDataProduct");
    private string OpenDataAsset => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "DimDataAsset");
    private string OpenDataProductDetail => QueryConstants.ServerlessQuery.FromTable(this.DomainModelDbName, "DataProduct");
    private string OpenDataProductOwners => QueryConstants.ServerlessQuery.FromTable(this.DomainModelDbName, "DataProductOwner");
    private string OpenDataProductStatus => QueryConstants.ServerlessQuery.FromTable(this.DomainModelDbName, "DataProductStatus");
    private string OpenRuleType => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "DimDQRuleType");
    private string OpenJobType => QueryConstants.ServerlessQuery.FromTable(this.DimensionalModelModelDbName, "DimDQJobType");

    public DataQualityScoreRecord ParseRow(IDataRecord row)
    {
        var DataProductId = row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductId).Name]?.ToString();
        var BusinessDomainId = row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.BusinessDomainId).Name]?.ToString();
        var DataAssetId = row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataAssetId).Name]?.ToString();
        if (!Guid.TryParse(DataProductId, out var g1) ||
            !Guid.TryParse(BusinessDomainId, out var g2) ||
            !Guid.TryParse(DataAssetId, out var g3)
            )
        {
            return null;
        }
        return new DataQualityScoreRecord()
        {
            Score = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.Score).Name]?.ToString()).AsFloat(),
            BusinessDomainId =
                BusinessDomainId.AsGuid(),
            DataProductId = DataProductId.AsGuid(),
            DataAssetId = DataAssetId.AsGuid(),
            DQJobId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DQJobId).Name]?.ToString()).AsGuid(),
            ExecutionTime = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.ExecutionTime).Name]?.ToString()).AsDateTime(),
            DataProductStatus = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductStatus).Name]?.ToString()),
            DataProductOwnerIds = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductOwnerIds).Name]?.ToString()),
            QualityDimension = this.QueryByDimension ? (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.QualityDimension).Name]?.ToString()) : null
        };
    }

    public IEnumerable<DataQualityScoreEntity> Finalize(dynamic records)
    {
        IList<DataQualityScoreEntity> entityList = new List<DataQualityScoreEntity>();
        if (records == null)
        {
            return entityList;
        }

        entityList = (records as IList<DataQualityScoreRecord>)
            .Select(item => new DataQualityScoreEntity()
            {
                DataAssetId = item.DataAssetId,
                BusinessDomainId = item.BusinessDomainId,
                DataProductId = item.DataProductId,
                DQJobId = item.DQJobId,
                ExecutionTime = item.ExecutionTime,
                Score = item.Score / 100,
                DataProductStatus = item.DataProductStatus,
                DataProductOwners = item.DataProductOwnerIds.Split(','),
                QualityDimension = item.QualityDimension
            }).ToList();
        return entityList;
    }
}

internal class ScoreRecordGroupComparor : IEqualityComparer<DataQualityScoreRecord>
{
    public bool Equals(DataQualityScoreRecord x, DataQualityScoreRecord y)
    {
        return x.BusinessDomainId == y.BusinessDomainId && x.DataProductId == y.DataProductId && x.DataAssetId == y.DataAssetId;
    }

    public int GetHashCode(DataQualityScoreRecord obj)
    {
        return obj.BusinessDomainId.GetHashCode() ^ obj.DataProductId.GetHashCode() ^ obj.DataAssetId.GetHashCode();
    }
}
