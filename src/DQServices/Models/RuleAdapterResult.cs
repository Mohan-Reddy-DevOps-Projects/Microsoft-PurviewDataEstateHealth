namespace Microsoft.Purview.DataEstateHealth.DHModels.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
using System.Collections.Generic;

public class RuleAdapterResult
{
    public List<CustomTruthRuleWrapper> CustomRules { get; set; }
}
