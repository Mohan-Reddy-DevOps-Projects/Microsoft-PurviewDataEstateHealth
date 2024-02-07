// <copyright file="ActionGroupedRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;

    public class ActionGroupedRequest
    {
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
        public ActionFilters Filters { get; set; }

        [JsonProperty("groupBy", NullValueHandling = NullValueHandling.Ignore)]
        public string groupBy { get; set; }
    }
}
