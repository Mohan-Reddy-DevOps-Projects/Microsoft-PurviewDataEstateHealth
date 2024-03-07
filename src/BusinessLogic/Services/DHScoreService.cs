namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHScoreService(DHScoreRepository dhScoreRepository, DHControlRepository dhControlRepository, DHAssessmentRepository mqAssessmentRepository, DHActionInternalService dHActionInternalService, IDataEstateHealthRequestLogger logger)
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
                    try
                    {
                        await Task.WhenAll(
                            this.StoreScoreAsync(controlId, assessmentId, controlNode, assessment, scores, computingJobId),
                            this.GenerateActionAsync(controlNode, assessment, scores, computingJobId)
                        ).ConfigureAwait(false);
                    }
                    catch (AggregateException ae)
                    {
                        foreach (var e in ae.InnerExceptions)
                        {
                            logger.LogError(e.Message);
                        }
                        throw new Exception(ae.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                        throw new Exception(ex.ToString());
                    }
                }
                else
                {
                    throw new InvalidCastException($"Control with id {controlId} is not a node!");
                }
            }
        }

        public Task<IEnumerable<DHScoreAggregatedByControl>> QueryScoreGroupByControl(IEnumerable<string> controlIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
        {
            return dhScoreRepository.QueryScoreGroupByControl(controlIds, domainIds, recordLatestCounts, start, end, status);
        }

        public Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryScoreGroupByControlGroup(IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
        {
            return dhScoreRepository.QueryScoreGroupByControlGroup(controlGroupIds, domainIds, recordLatestCounts, start, end, status);
        }

        private async Task StoreScoreAsync(string controlId, string assessmentId, DHControlNodeWrapper controlNode, DHAssessmentWrapper assessment, IEnumerable<DHRawScore> scores, string computingJobId)
        {
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
                                            DataProductStatus = x.EntityPayload[DQOutputFields.DP_STATUS]?.ToString() ?? throw new InvalidOperationException("Data product status not found in entity payload!"),
                                            DataProductId = x.EntityId ?? throw new InvalidOperationException("Data product id not found in entity payload!"),
                                            DataProductOwners = x.EntityPayload[DQOutputFields.DP_OWNER_IDS]?.ToString().Split(",") ?? throw new InvalidOperationException("Data product owner ids not found in entity payload!")
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

        private async Task GenerateActionAsync(DHControlNodeWrapper control, DHAssessmentWrapper assessment, IEnumerable<DHRawScore> scores, string computingJobId)
        {
            List<DataHealthActionWrapper> actions = new List<DataHealthActionWrapper>();
            var controlGroupId = control.GroupId;
            var controlGroup = await dhControlRepository.GetByIdAsync(controlGroupId).ConfigureAwait(false);
            var findingType = controlGroup?.Name ?? "";
            var findingSubType = control.Name;
            var targetEntityType = (DataHealthActionTargetEntityType?)assessment.TargetEntityType;

            if (targetEntityType == null)
            {
                throw new NotImplementedException($"Target entity type {assessment.TargetEntityType?.ToString()} is not supported in action center yet!");
            }
            foreach (var score in scores)
            {
                var entityOwners = new List<string>();
                var owners = (string?)score.EntityPayload[DQOutputFields.DP_OWNER_IDS] ?? "";
                if (!string.IsNullOrEmpty(owners))
                {
                    entityOwners.AddRange(owners.Split(","));
                }
                var targetEntityId = (string?)score.EntityPayload[DQOutputFields.DP_ID];
                var businessDomainId = (string?)score.EntityPayload[DQOutputFields.BD_ID];
                if (businessDomainId != null && targetEntityId != null)
                {
                    foreach (var scoreUnit in score.Scores)
                    {
                        if (scoreUnit.Score == 0)
                        {
                            var assessmentRuleId = scoreUnit.AssessmentRuleId;
                            var matchedAssessment = assessment.Rules.FirstOrDefault((rule) =>
                            {
                                return rule.Id == assessmentRuleId;
                            });
                            if (matchedAssessment?.ActionProperties != null)
                            {
                                var action = new DataHealthActionWrapper()
                                {
                                    Category = DataHealthActionCategory.HealthControl,
                                    Severity = matchedAssessment.ActionProperties?.Severity ?? DataHealthActionSeverity.Medium,
                                    FindingId = assessmentRuleId,
                                    FindingName = matchedAssessment.ActionProperties?.Name ?? "",
                                    Reason = matchedAssessment.ActionProperties?.Reason ?? "",
                                    Recommendation = matchedAssessment.ActionProperties?.Recommendation ?? "",
                                    FindingType = findingType,
                                    FindingSubType = findingSubType,
                                    TargetEntityType = (DataHealthActionTargetEntityType)targetEntityType,
                                    TargetEntityId = targetEntityId,
                                    AssignedTo = entityOwners,
                                    DomainId = businessDomainId,
                                    ExtraProperties = new ActionExtraPropertiesWrapper()
                                    {
                                        Type = DHActionType.ControlAction,
                                        Data = new JObject
                                        {
                                            ["jobId"] = computingJobId
                                        }
                                    }
                                };
                                actions.Add(action);
                            }
                        }
                    }
                }
            }
            if (actions.Any())
            {
                await dHActionInternalService.CreateActionsAsync(actions);
            }
        }
    }
}
