namespace Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using System.Collections.Generic;

public enum JoinRequirement
{
    Term
}

public class RuleAdapterContext
{
    public RuleAdapterContext()
    {
        this.joinRequirements = new List<JoinRequirement>();
    }


    public List<JoinRequirement> joinRequirements { get; }
}
