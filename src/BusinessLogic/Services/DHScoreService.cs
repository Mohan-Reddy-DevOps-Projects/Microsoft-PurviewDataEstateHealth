namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHScoreService(DHScoreRepository dhScoreRepository, DHControlRepository dhControlRepository, DHAssessmentRepository mqAssessmentRepository, IDataEstateHealthRequestLogger logger)
    {
        public async Task<IBatchResults<DHScoreBaseWrapper>> ListScoresAsync()
        {
            var scores = await dhScoreRepository.GetAllAsync().ConfigureAwait(false);

            return new BatchResults<DHScoreBaseWrapper>(scores, scores.Count());
        }

        public async Task ProcessControlComputingResultsAsync(string controlId, string computingJobId, IEnumerable<DHRawScore> scores)
        {
            using (logger.LogElapsed($"{nameof(ProcessControlComputingResultsAsync)}, controlId = {controlId}, computingJobId = {computingJobId}, rawScoreCount = {scores.Count()}"))
            {
                var control = await dhControlRepository.GetByIdAsync(controlId).ConfigureAwait(false) ?? throw new InvalidOperationException($"Control with id {controlId} not found!");
                if (control is DHControlNodeWrapper controlNode)
                {
                    var assessmentId = controlNode.AssessmentId ?? throw new InvalidOperationException($"Control with id {controlId} has no assessment id!");
                    var assessment = await mqAssessmentRepository.GetByIdAsync(assessmentId).ConfigureAwait(false) ?? throw new InvalidOperationException($"Assessment with id {assessmentId} not found!");
                    var aggregation = assessment.AggregationWrapper ?? throw new InvalidOperationException($"Assessment with id {assessmentId} has no aggregation!");
                    var currentTime = DateTime.UtcNow;
                    switch (aggregation)
                    {
                        case DHAssessmentSimpleAggregationWrapper simpleAggregation:
                            switch (simpleAggregation.AggregationType)
                            {
                                case DHAssessmentSimpleAggregationType.Average:
                                    var scoreWrappers = scores.Select(x =>
                                    {
                                        switch (assessment.TargetEntityType)
                                        {
                                            case DHAssessmentTargetEntityType.DataProduct:
                                                return new DHDataProductScoreWrapper
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ControlId = controlId,
                                                    ControlGroupId = controlNode.GroupId,
                                                    ComputingJobId = computingJobId,
                                                    Time = currentTime,
                                                    Scores = x.Scores,
                                                    AggregatedScore = x.Scores.Average(scoreUnit => scoreUnit.Score),
                                                    DataProductDomainId = x.EntityPayload[DQOutputFields.BD_ID]?.ToString() ?? throw new InvalidOperationException("Data product domain id not found in entity payload!"),
                                                    DataProductId = x.EntityId ?? throw new InvalidOperationException("Data product id not found in entity payload!"),
                                                    DataProductOwners = []
                                                    // TODO will get this after join
                                                    // DataProductOwners = x.EntityPayload["contacts"]?["owner"]?.OfType<JObject>().Select(x => ContactItemWrapper.Create(x)).ToList()
                                                };
                                            default:
                                                throw new NotImplementedException($"Target entity type {assessment.TargetEntityType} not supported yet!");
                                        }
                                    }).ToList();
                                    await dhScoreRepository.AddAsync(scoreWrappers).ConfigureAwait(false);
                                    break;
                                default:
                                    throw new NotImplementedException($"Simple aggregation type {simpleAggregation.AggregationType} not supported yet!");
                            }
                            break;
                        default:
                            throw new NotImplementedException($"Aggregation type {aggregation.Type} not supported yet!");
                    }
                }
                else
                {
                    throw new InvalidCastException($"Control with id {controlId} is not a node!");
                }
            }
        }

        public Task<IEnumerable<DHScoreAggregatedByControl>> Query(IEnumerable<string>? domainIds, IEnumerable<string>? controlIds, int? recordLatestCounts, DateTime? start, DateTime? end)
        {
            return dhScoreRepository.Query(domainIds, controlIds, recordLatestCounts, start, end);
        }
    }
}
