namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System.Collections.Generic;

public class ScorePayload
{
    public required string EntityId;
    public required IEnumerable<DHScoreUnitWrapper> scores;
}
