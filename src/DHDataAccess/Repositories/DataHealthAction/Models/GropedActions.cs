namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public class GroupedActions
{
    public static IEnumerable<GroupedActions> ToGroupedActions(string groupBy, IEnumerable<DataHealthActionWrapper> actions)
    {
        return actions
            .GroupBy(item => item.JObject.ContainsKey(groupBy) ? item.JObject[groupBy] : null)
            .Where(group => group.Key != null)
            .Select(group => new GroupedActions
            {
                GroupName = group.Key?.ToString() ?? "",
                Items = group.Select((item) => new JObject {
                    { DataHealthActionWrapper.keyFindingType, item.FindingType},
                    { DataHealthActionWrapper.keyFindingSubType, item.FindingSubType},
                    { DataHealthActionWrapper.keyFindingName, item.FindingName},
                    { DataHealthActionWrapper.keySeverity, item.Severity.ToString()},
                    { DataHealthActionWrapper.keyCategory, item.Category.ToString()},
                })
            });
    }
    [JsonProperty("groupName", Required = Required.Always)]
    public required string GroupName { get; set; }

    [JsonProperty("items", Required = Required.Always)]
    public required IEnumerable<JToken> Items { get; set; }
}
