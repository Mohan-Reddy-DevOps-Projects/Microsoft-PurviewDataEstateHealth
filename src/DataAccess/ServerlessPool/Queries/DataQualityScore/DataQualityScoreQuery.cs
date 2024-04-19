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
    public string QueryPath => $"{this.ContainerPath}/DimensionalModel/FactDataQuality/";

    public string Query
    {
        get => @"
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
                    " + QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @"AS [result]
                        JOIN (SELECT DQRuleTypeId, QualityDimension " + QueryConstants.ServerlessQuery.OpenRowSet(this.RuleTypeQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @"AS [result]) RuleType ON [result].DQRuleTypeId = RuleType.DQRuleTypeId
                        JOIN (SELECT JobTypeId, JobTypeDisplayName " + QueryConstants.ServerlessQuery.OpenRowSet(this.JobTypeQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @"AS [result]) JobType ON [result].JobTypeId = JobType.JobTypeId
                    " + this.FilterClause + @" AND JobTypeDisplayName = 'DQ'
                    GROUP BY BusinessDomainId, DataProductId, DataAssetId, DQJobSourceId
            ) TMP1
        ) TMP2 WHERE row_num = 1
    ) DQFact
JOIN (SELECT BusinessDomainId, BusinessDomainSourceId " + QueryConstants.ServerlessQuery.OpenRowSet(this.BusinessDomainQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result]) BD ON DQFact.BusinessDomainId = BD.BusinessDomainId
JOIN (SELECT DataProductId, DataProductSourceId " + QueryConstants.ServerlessQuery.OpenRowSet(this.DataProductQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result]) DP ON DQFact.DataProductId = DP.DataProductId
JOIN (SELECT DataAssetId, DataAssetSourceId " + QueryConstants.ServerlessQuery.OpenRowSet(this.DataAssetQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result]) DA ON DQFact.DataAssetId = DA.DataAssetId
JOIN (SELECT DataProductId, DataProductStatusID " + QueryConstants.ServerlessQuery.OpenRowSet(this.DataProductDetailQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result]) DPDetail ON DP.DataProductSourceId = DPDetail.DataProductId
JOIN (SELECT DataProductStatusID, DataProductStatusDisplayName " + QueryConstants.ServerlessQuery.OpenRowSet(this.DataProductStatusQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result]) DPStatus ON DPDetail.DataProductStatusID = DPStatus.DataProductStatusID
JOIN (SELECT DataProductId, STRING_AGG(DataProductOwnerId, ',') AS DataProductOwnerIds " + QueryConstants.ServerlessQuery.OpenRowSet(this.DataProductOwnersQueryPath, QueryConstants.ServerlessQuery.DeltaFormat) + @" AS [result] GROUP BY DataProductId) DPOwner ON DP.DataProductSourceId = DPOwner.DataProductId
";
    }

    private string BusinessDomainQueryPath => $"{this.ContainerPath}/DimensionalModel/DimBusinessDomain/";
    private string DataProductQueryPath => $"{this.ContainerPath}/DimensionalModel/DimDataProduct/";
    private string DataAssetQueryPath => $"{this.ContainerPath}/DimensionalModel/DimDataAsset/";
    private string DataProductDetailQueryPath => $"{this.ContainerPath}/DomainModel/DataProduct/";
    private string DataProductOwnersQueryPath => $"{this.ContainerPath}/DomainModel/DataProductOwner/";
    private string DataProductStatusQueryPath => $"{this.ContainerPath}/DomainModel/DataProductStatus/";
    private string RuleTypeQueryPath => $"{this.ContainerPath}/DimensionalModel/DimDQRuleType/";
    private string JobTypeQueryPath => $"{this.ContainerPath}/DimensionalModel/DimDQJobType/";

    public DataQualityScoreRecord ParseRow(IDataRecord row)
    {
        var DataProductId = row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductId).Name]?.ToString();
        if (!Guid.TryParse(DataProductId, out var result))
        {
            return null;
        }
        return new DataQualityScoreRecord()
        {
            Score = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.Score).Name]?.ToString()).AsFloat(),
            BusinessDomainId =
                (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.BusinessDomainId).Name]?.ToString()).AsGuid(),
            DataProductId = DataProductId.AsGuid(),
            DataAssetId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataAssetId).Name]?.ToString()).AsGuid(),
            DQJobId = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DQJobId).Name]?.ToString()).AsGuid(),
            ExecutionTime = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.ExecutionTime).Name]?.ToString()).AsDateTime(),
            DataProductStatus = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductStatus).Name]?.ToString()),
            DataProductOwnerIds = (row[GetCustomAttribute<DataColumnAttribute, DataQualityScoreRecord>(x => x.DataProductOwnerIds).Name]?.ToString())
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
                DataProductOwners = item.DataProductOwnerIds.Split(',')
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
