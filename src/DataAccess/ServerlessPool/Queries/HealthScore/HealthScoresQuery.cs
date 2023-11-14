// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using static Microsoft.Azure.Purview.DataEstateHealth.Common.QueryUtils;

[ServerlessQuery(typeof(HealthScoreEntity))]
internal class HealthScoresQuery : BaseQuery, IServerlessQueryRequest<HealthScoreRecord, HealthScoreEntity>
{
    public string QueryPath => $"{this.ContainerPath}/Sink/HealthScores/";

    public string Query
    {
        get => "SELECT ScoreKind, Name, Description, ActualValue, BusinessDomainId " +
               QueryConstants.ServerlessQuery.OpenRowSet(this.QueryPath, QueryConstants.ServerlessQuery.DeltaFormat) +
               "WITH(ScoreKind nvarchar(128), Name nvarchar(128), Description nvarchar(1024), ActualValue float," +
               "BusinessDomainId  nvarchar(64))" +
               QueryConstants.ServerlessQuery.AsRows + this.FilterClause;
    }

    public HealthScoreRecord ParseRow(IDataRecord row)
    {
        return new HealthScoreRecord()
        {
            BusinessDomainId =
                Guid.Parse(row[GetCustomAttribute<DataColumnAttribute, HealthScoreRecord>(x => x.BusinessDomainId).Name]?.ToString()),
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

        foreach (HealthScoreRecord record in records)
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
