// <copyright file="QueryBaseRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class QueryBaseRequest
    {
        [JsonProperty("continuationToken", NullValueHandling = NullValueHandling.Ignore)]
        public string ContinuationToken { get; set; }

        [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
        public int PageSize { get; set; } = 100;

        [JsonProperty("orderBy", NullValueHandling = NullValueHandling.Ignore)]
        public List<OrderBy> OrderBy { get; set; }
    }

    public class OrderBy
    {
        [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        [JsonProperty("direction", NullValueHandling = NullValueHandling.Ignore)]
        public string Direction { get; set; }
    }
}
