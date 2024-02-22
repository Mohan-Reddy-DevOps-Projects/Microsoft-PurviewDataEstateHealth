namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHScoreService(DHScoreRepository dhScoreRepository, DHControlRepository dhControlRepository, MQAssessmentRepository mqAssessmentRepository)
    {
        public async Task<IBatchResults<DHScoreWrapper>> ListScoresAsync()
        {
            var scores = await dhScoreRepository.GetAllAsync().ConfigureAwait(false);

            return new BatchResults<DHScoreWrapper>(scores, scores.Count());
        }

        public async Task ProcessControlComputingResultsAsync(string controlId, string computingJobId, IEnumerable<DHRawScore> scores)
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
                    case MQAssessmentSimpleAggregationWrapper simpleAggregation:
                        switch (simpleAggregation.AggregationType)
                        {
                            case MQAssessmentSimpleAggregationType.Average:
                                var scoreWrappers = scores.Select(x =>
                                {
                                    var scoreWrapper = new DHScoreWrapper
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ControlId = controlId,
                                        ControlGroupId = controlNode.GroupId,
                                        ComputingJobId = computingJobId,
                                        Time = currentTime,
                                        Scores = x.Scores,
                                        AggregatedScore = x.Scores.Average(scoreUnit => scoreUnit.Score)
                                        // TODO: add entity snapshot
                                        // TODO: add entity domain id
                                    };
                                    return scoreWrapper;
                                });
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

        public Task<IEnumerable<DHScoreAggregatedByControl>> Query(IEnumerable<string>? domainIds, IEnumerable<string>? controlIds, int? recordLatestCounts, DateTime? start, DateTime? end)
        {
            return dhScoreRepository.Query(domainIds, controlIds, recordLatestCounts, start, end);
        }
    }
}
