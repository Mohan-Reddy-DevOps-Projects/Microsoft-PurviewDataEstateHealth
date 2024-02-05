namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ScoreProcessingService(DHControlRepository dhControlRepository, MQAssessmentRepository mqAssessmentRepository, DHScoreRepository dhScoreRepository)
{
    public async Task ProcessControlComputingResultsAsync(string controlId, string computingJobId, IEnumerable<ScorePayload> scores)
    {
        var control = await dhControlRepository.GetByIdAsync(controlId).ConfigureAwait(false) ?? throw new InvalidOperationException($"Control with id {controlId} not found!");
        if (control is DHControlNodeWrapper controlNode)
        {
            var assessmentId = controlNode.AssessmentId ?? throw new InvalidOperationException($"Control with id {controlId} has no assessment id!");
            var assessment = await mqAssessmentRepository.GetByIdAsync(assessmentId).ConfigureAwait(false) ?? throw new InvalidOperationException($"Assessment with id {assessmentId} not found!");
            var aggregation = assessment.AggregationWrapper ?? throw new InvalidOperationException($"Assessment with id {assessmentId} has no aggregation!");
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
                                    ControlId = controlId,
                                    ComputingJobId = computingJobId,
                                    Time = DateTime.UtcNow,
                                    Score = x.scores,
                                    AggregatedScore = x.scores.Average(scoreUnit => scoreUnit.Score)
                                    // TODO: add entity snapshot
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
}

public class ScorePayload
{
    public required string EntityId;
    public required IEnumerable<DHScoreUnitWrapper> scores;
}
