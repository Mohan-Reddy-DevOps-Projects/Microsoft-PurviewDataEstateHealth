// <copyright file="ActionListRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;

    public class ActionQueryRequest : QueryBaseRequest
    {
        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
        public ActionFiltersPayload Filters { get; set; }
    }
}
