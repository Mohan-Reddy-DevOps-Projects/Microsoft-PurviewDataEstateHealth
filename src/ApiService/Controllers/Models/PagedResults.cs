// <copyright file="PagedResults.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PagedResults
    {
        public static PagedResults FromBatchResults(
            IEnumerable<BaseEntityWrapper> batchResults)
        {
            if (batchResults == null)
            {
                throw new ArgumentNullException(nameof(batchResults));
            }

            return new PagedResults()
            {
                Value = batchResults.Select((item) => item.JObject),
            };
        }

        [JsonProperty("value", Required = Required.Always)]
        public IEnumerable<JToken> Value { get; set; }
    }
}