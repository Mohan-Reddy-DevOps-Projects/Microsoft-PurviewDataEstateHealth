namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System.Collections.Generic;

public static class ActionsSorter
{
    public static readonly Dictionary<string, string> SortFields = new Dictionary<string, string>
    {
        { ActionSystemDataWrapper.keyCreatedAt, "SystemInfo.CreatedAt" },
        { DataHealthActionWrapper.keyFindingName, "FindingName" },
        { DataHealthActionWrapper.keyFindingType, "FindingType" },
        { DataHealthActionWrapper.keyFindingSubType, "FindingSubType" },
        { DataHealthActionWrapper.keySeverity, "Severity" },
        { DataHealthActionWrapper.keyStatus, "Status" },
    };
}
