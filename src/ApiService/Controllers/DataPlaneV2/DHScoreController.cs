#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Newtonsoft.Json;

/// <summary>
/// Health Reports controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/scores")]
public class DHScoreController(DHScoreService dhScoreService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListScoresAsync()
    {
        var batchResults = await dhScoreService.ListScoresAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("queryControlScores")]
    public async Task<ActionResult> QueryControlScoresAsync(
        [FromBody] QueryControlScoresRequest queryScoresRequest)
    {
        Validate(queryScoresRequest);
        var results = await dhScoreService.QueryScoreGroupByControl(queryScoresRequest.ControlIds, queryScoresRequest.DomainIds, queryScoresRequest.RecordLatestCounts, queryScoresRequest.RecordTimeRange?.Start, queryScoresRequest.RecordTimeRange?.End, queryScoresRequest.Status).ConfigureAwait(false);
        return this.Ok(results);
    }

    [HttpPost]
    [Route("queryControlGroupScores")]
    public async Task<ActionResult> QueryControlGroupScoresAsync(
        [FromBody] QueryControlGroupScoresRequest queryScoresRequest)
    {
        Validate(queryScoresRequest);
        var results = await dhScoreService.QueryScoreGroupByControlGroup(queryScoresRequest.ControlGroupIds, queryScoresRequest.DomainIds, queryScoresRequest.RecordLatestCounts, queryScoresRequest.RecordTimeRange?.Start, queryScoresRequest.RecordTimeRange?.End, queryScoresRequest.Status).ConfigureAwait(false);
        return this.Ok(results);
    }

    private static void Validate(QueryScoresRequestBase queryScoresRequest)
    {
        if (queryScoresRequest.RecordLatestCounts == null && queryScoresRequest.RecordTimeRange == null)
        {
            throw new InvalidRequestException("Either recordLatestCounts or recordTimeRange must be specified in the request");
        }

        if (queryScoresRequest.RecordTimeRange != null && queryScoresRequest.RecordTimeRange.Start > queryScoresRequest.RecordTimeRange.End)
        {
            throw new InvalidRequestException("Start time must be less than or equal to end time");
        }
    }
}

public record QueryScoresRequestBase
{
    [JsonProperty("domainIds")]
    public IEnumerable<string>? DomainIds { get; set; }

    [JsonProperty("recordLatestCounts")]
    public int? RecordLatestCounts { get; set; }

    [JsonProperty("recordTimeRange")]
    public QueryScoresRequestTimeRange? RecordTimeRange { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}

public record QueryControlScoresRequest : QueryScoresRequestBase
{
    [JsonProperty("controlIds")]
    public required IEnumerable<string> ControlIds { get; set; }
}

public record QueryControlGroupScoresRequest : QueryScoresRequestBase
{
    [JsonProperty("controlGroupIds")]
    public required IEnumerable<string> ControlGroupIds { get; set; }
}

public record QueryScoresRequestTimeRange
{
    [JsonProperty("start")]
    public DateTime Start { get; set; }

    [JsonProperty("end")]
    public DateTime End { get; set; }
}
