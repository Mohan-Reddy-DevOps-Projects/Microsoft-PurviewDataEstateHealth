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
        public required string JobPartition { get; set; }

        [JsonProperty("jobId")]
        public required string JobId { get; set; }
    }

    public class GenevaActionGetBackgroundJobDetailRequestPayload
    {
        [JsonProperty("jobPartition")]
        public required string JobPartition { get; set; }

        [JsonProperty("jobId")]
        public required string JobId { get; set; }
    }

    public class GenevaActionTriggerScheduleRequestPayload
    {
        [JsonProperty("controlId")]
        public string ControlId { get; set; }
    }

    public class GenevaActionListMonitoringJobsRequestPayload
    {
        [JsonProperty("scheduleRunId")]
        public required string ScheduleRunId { get; set; }
    }

    public record KickOffCatalogBackfillRequest(
            [property: JsonProperty("accountIds")] List<string> AccountIds,
            [property: JsonProperty("batchAmount")] int BatchAmount,
            [property: JsonProperty("bufferTimeInMinutes")] int BufferTimeInMinutes);
}
