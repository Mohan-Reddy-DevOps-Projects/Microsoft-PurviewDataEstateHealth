// <copyright file="ActionFacetRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;

    public class ActionFacetRequest
    {
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
        public ActionFiltersPayload Filters { get; set; }

        [JsonProperty("facets", NullValueHandling = NullValueHandling.Ignore)]
        public List<FacetRequestObject> Facets { get; set; }
    }
}
