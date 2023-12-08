// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthControlRecord))]
internal class HealthControlQuery : BaseQuery, IServerlessQueryRequest<HealthControlRecord, HealthControlSqlEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/HealthScores/";

    public string Query
    {
        get => "SELECT ActualValue, C2_Ownership, C3_AuthoritativeSource, MetadataCompleteness, C5_Catalog, C6_Classification, C7_Access, C8_DataConsumptionPurpose, C12_Quality, Use, Quality, LastRefreshDate " +
            QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
            "WITH (ActualValue float, C2_Ownership float, C3_AuthoritativeSource float, MetadataCompleteness float, C5_Catalog float, C6_Classification float, C7_Access float, C8_DataConsumptionPurpose float, C12_Quality float, Use float, Quality float, LastRefreshDate DateTime2)" +
            QueryConstants.ServerlessQuery.AsRows;
    }

    public HealthControlRecord ParseRow(IDataRecord row)
    {
        return new HealthControlRecord()
        {
            ActualValue =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.ActualValue).Name].ToString()),
            C12_DataQuality =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C12_DataQuality).Name].ToString()),
            C2_Ownership =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C2_Ownership).Name].ToString()),
            C3_AuthoritativeSource =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C3_AuthoritativeSource).Name].ToString()),
            C5_Catalog =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C5_Catalog).Name].ToString()),
            C6_Classification =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C6_Classification).Name].ToString()),
            C7_Access =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C7_Access).Name].ToString()),
            C8_DataConsumptionPurpose =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.C8_DataConsumptionPurpose).Name].ToString()),
            Quality =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.Quality).Name].ToString()),
            Use =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.Use).Name].ToString()),
            MetadataCompleteness =
                Convert.ToDouble(row[GetCustomAttribute<DataColumnAttribute, HealthControlRecord>(x => x.MetadataCompleteness).Name].ToString()),
            LastRefreshedAt =
                (row[GetCustomAttribute<DataColumnAttribute, HealthActionsRecord>(x => x.LastRefreshedAt).Name]?.ToString()).AsDateTime()
        };
    }

    public IEnumerable<HealthControlSqlEntity> Finalize(dynamic records)
    {
        IList<HealthControlSqlEntity> entityList = new List<HealthControlSqlEntity>();
        if (records == null)
        {
            return entityList;
        }

        foreach (HealthControlRecord record in records)
        {
            //Add parent data governance
            entityList.Add(new HealthControlSqlEntity()
            {
                CurrentScore = record.ActualValue,
                AccessEntitlementScore = record.C7_Access,
                AuthoritativeDataSourceScore = record.C3_AuthoritativeSource,
                CatalogingScore = record.C5_Catalog,
                ClassificationScore = record.C6_Classification,
                DataConsumptionPurposeScore = record.C8_DataConsumptionPurpose,
                DataQualityScore = record.C12_DataQuality,
                MetadataCompletenessScore = record.MetadataCompleteness,
                OwnershipScore = record.C2_Ownership,
                QualityScore = record.Quality,
                UseScore = record.Use,
                LastRefreshedAt = record.LastRefreshedAt
            });
        }

        return entityList;
    }
}
