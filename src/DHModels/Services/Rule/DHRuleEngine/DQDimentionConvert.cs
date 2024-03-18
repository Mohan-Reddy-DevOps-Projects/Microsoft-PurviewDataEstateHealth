namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DQDimentionConvert
{
    public static string ConvertCheckPointToDQDimension(DHCheckPoint? dHCheckPoint)
    {
        switch (dHCheckPoint)
        {
            case DHCheckPoint.DataQualityAccuracyScore:
                return "Accuracy";
            case DHCheckPoint.DataQualityConformityScore:
                return "Conformity";
            case DHCheckPoint.DataQualityConsistencyScore:
                return "Consistency";
            case DHCheckPoint.DataQualityCompletenessScore:
                return "Completeness";
            case DHCheckPoint.DataQualityUniquenessScore:
                return "Uniqueness";
            case DHCheckPoint.DataQualityTimelinessScore:
                return "Timeliness";
            default:
                return "";
        }
    }
}
