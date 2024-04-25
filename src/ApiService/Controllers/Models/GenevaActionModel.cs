// <copyright file="GenevaActionModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models
{
    using Newtonsoft.Json;

    public class GenevaActionResponse
    {
        [JsonProperty("code")]
        public required string Code { get; set; }

        [JsonProperty("message")]
        public required string Message { get; set; }
    }

    public class GenevaActionTriggerBackgroundJobRequestPayload
    {
        [JsonProperty("jobPartition")]
        public string JobPartition { get; set; }

        [JsonProperty("jobId")]
        public string JobId { get; set; }
    }

    public class GenevaActionGetBackgroundJobDetailRequestPayload
    {
        [JsonProperty("jobPartition")]
        public string JobPartition { get; set; }

        [JsonProperty("jobId")]
        public string JobId { get; set; }
    }
}
