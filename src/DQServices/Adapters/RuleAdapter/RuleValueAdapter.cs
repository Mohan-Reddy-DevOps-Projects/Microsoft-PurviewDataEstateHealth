namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
public class RuleValueAdapter
{
    public static string ToDqExpression(string userInputValue)
    {
        if (userInputValue == null)
        {
            return string.Empty;
        }

        // TODO
        return userInputValue;
    }
}
