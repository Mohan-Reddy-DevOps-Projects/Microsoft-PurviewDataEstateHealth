// <copyright file="ActionFilters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;

    public class ActionFiltersPayload
    {
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Status { get; set; }

        //Todo: add it later, there are issue while using Intersect in Cosmosdb
        [JsonProperty("assignedTo", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AssignedTo { get; set; }

        [JsonProperty("findingTypes", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FindingTypes { get; set; }

        [JsonProperty("findingSubTypes", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FindingSubTypes { get; set; }

        [JsonProperty("findingNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FindingNames { get; set; }

        [JsonProperty("severity", NullValueHandling = NullValueHandling.Ignore)]
        public string Severity { get; set; }

        [JsonProperty("targetEntityType", NullValueHandling = NullValueHandling.Ignore)]
        public string TargetEntityType { get; set; }

        [JsonProperty("targetEntityIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> TargetEntityIds { get; set; }

        [JsonProperty("createTimeRange", NullValueHandling = NullValueHandling.Ignore)]
        public CreateTimeRangeFilterPayload CreateTimeRange { get; set; }
    }

    public class CreateTimeRangeFilterPayload
    {
        [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Start { get; set; }

        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? End { get; set; }
    }
}
