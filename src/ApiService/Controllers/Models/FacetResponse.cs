// <copyright file="FacetResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class FacetResponse
    {
        [JsonProperty("facets", Required = Required.Always)]
        public Dictionary<string, List<FacetResponseObject>> Facets { get; set; } = new Dictionary<string, List<FacetResponseObject>>();
    }

    public class FacetResponseObject
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public int? Count { get; set; }

        public FacetResponseObject(string value, int? count)
        {
            this.Value = value;
            this.Count = count;
        }
    }
}
