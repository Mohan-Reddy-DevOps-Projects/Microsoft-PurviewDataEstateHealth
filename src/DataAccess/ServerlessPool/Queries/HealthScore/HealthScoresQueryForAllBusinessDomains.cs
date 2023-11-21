// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthScoreRecordForAllBusinessDomains))]
internal class HealthScoresQueryForAllBusinessDomains : BaseQuery, IServerlessQueryRequest<HealthScoreRecordForAllBusinessDomains, HealthScoreEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/HealthScores/";

    public string Query
    {
        get => "SELECT ScoreKind, Name, Description, ActualValue " +
               QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
               "WITH(ScoreKind nvarchar(128), Name nvarchar(128), Description nvarchar(1024), ActualValue float" +
               QueryConstants.ServerlessQuery.AsRows;
    }

    public HealthScoreRecordForAllBusinessDomains ParseRow(IDataRecord row)
    {
        return new HealthScoreRecordForAllBusinessDomains()
        { 
            Name =
                row[GetCustomAttribute<DataColumnAttribute, HealthScoreRecord>(x => x.Name).Name]?.ToString(),
            Description =
                 row[GetCustomAttribute<DataColumnAttribute, HealthScoreRecord>(x => x.Description).Name]?.ToString(),
            Kind =
                 row[GetCustomAttribute<DataColumnAttribute, HealthScoreRecord>(x => x.Kind).Name]?.ToString(),
            ActualValue =
                 row[GetCustomAttribute<DataColumnAttribute, HealthScoreRecord>(x => x.ActualValue).Name].ToString().AsFloat(),
        };
    }

    public IEnumerable<HealthScoreEntity> Finalize(dynamic records)
    {
        IList<HealthScoreEntity> entityList = new List<HealthScoreEntity>();

        foreach (HealthScoreRecordForAllBusinessDomains record in records)
        {
            var performanceIndicatorRules = new List<PerformanceIndicatorRules>()
            {
                new PerformanceIndicatorRules()
                {
                    RuleOrder = 0,
                    MinValue = 0,
                    MaxValue = 25,
                    DisplayText = "Unhealthy",
                    DefaultColor = "#D13438"
                },
                new PerformanceIndicatorRules()
                {
                    RuleOrder = 1,
                    MinValue = 26,
                    MaxValue = 50,
                    DisplayText = "Medium",
                    DefaultColor = "#E8CC78"
                },
                new PerformanceIndicatorRules()
                {
                    RuleOrder = 2,
                    MinValue = 51,
                    MaxValue = 100,
                    DisplayText = "Healthy",
                    DefaultColor = "#80D091"
                }
            };

            HealthScoreKind ScoreKind = Enum.Parse<HealthScoreKind>(record.Kind);

            switch (ScoreKind)
            {
                case HealthScoreKind.DataQuality:
                    entityList.Add(new DataQualityHealthScoreEntity()
                    {
                        Name = record.Name,
                        ScoreKind = ScoreKind,
                        Description = record.Description,
                        ActualValue = record.ActualValue,
                        TargetValue = 50,
                        MeasureUnit = "%",
                        PerformanceIndicatorRules = performanceIndicatorRules,

                    });
                    break;
                case HealthScoreKind.DataGovernance:
                    entityList.Add(new DataGovernanceHealthScoreEntity()
                    {
                        Name = record.Name,
                        ScoreKind = ScoreKind,
                        Description = record.Description,
                        ActualValue = record.ActualValue,
                        TargetValue = 50,
                        MeasureUnit = "%",
                        PerformanceIndicatorRules = performanceIndicatorRules,
                    });
                    break;
            }
        }

        return entityList;
    }
}
