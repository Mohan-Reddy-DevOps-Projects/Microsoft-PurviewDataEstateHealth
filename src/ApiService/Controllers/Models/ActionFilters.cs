// <copyright file="ActionFilters.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ActionFilters
    {
        [JsonProperty("findingName", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> FindingName { get; set; }
    }
}
