namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using System.Collections.Generic;

public class DataQualityOutputAdapter
{
    public static IEnumerable<ScorePayload> ToScorePayload()
    {
        var result = new List<ScorePayload>();

        return result;
    }
}
