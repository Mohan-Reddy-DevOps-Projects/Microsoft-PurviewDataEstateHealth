namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

public class RuleValueAdapter
{
    public static string ToDqExpression(DHCheckPoint checkPoint, string userInputValue)
    {
        if (userInputValue == null)
        {
            return string.Empty;
        }

        // TODO
        return checkPoint switch
        {
            DHCheckPoint.DataProductStatus => $"'{userInputValue}'",
            _ => userInputValue
        };
    }
}
