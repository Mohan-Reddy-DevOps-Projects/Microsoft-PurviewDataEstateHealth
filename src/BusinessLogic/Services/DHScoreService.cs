namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Output;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHScoreService(
        DHScoreRepository dhScoreRepository,
        DHControlRepository dhControlRepository,
        DHAssessmentRepository mqAssessmentRepository,
        DHActionInternalService dHActionInternalService,
        IDataEstateHealthRequestLogger logger,
        IRequestHeaderContext requestHeaderContext)
    {
        public async Task<IBatchResults<DHScoreBaseWrapper>> ListScoresAsync()
        {
            var scores = await dhScoreRepository.GetAllAsync().ConfigureAwait(false);

            return new BatchResults<DHScoreBaseWrapper>(scores, scores.Count());
        }

        public async Task ProcessControlComputingResultsAsync(DHComputingJobWrapper job, IEnumerable<DHRawScore> scores, bool isRetry = false)
        {
            var controlId = job.ControlId;
            var computingJobId = job.Id;

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
                            this.StoreScoreAsync(job, assessmentId, controlNode, assessment, scores),
                            this.GenerateActionAsync(controlNode, assessment, scores, computingJobId, isRetry)
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

        public async Task DeprovisionForScoresAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForScoresAsync)}: Deprovision scores"))
            {
                await dhScoreRepository.DeprovisionAsync().ConfigureAwait(false);
            }
        }

        private async Task StoreScoreAsync(DHComputingJobWrapper job, string assessmentId, DHControlNodeWrapper controlNode, DHAssessmentWrapper assessment, IEnumerable<DHRawScore> scores)
        {
            var controlId = job.ControlId;
            var computingJobId = job.Id;
            var scheduleRunId = job.ScheduleRunId;

            var aggregation = assessment.AggregationWrapper ?? throw new InvalidOperationException($"Assessment with id {assessmentId} has no aggregation!");
            var currentTime = DateTime.UtcNow;
            switch (aggregation)
            {
                case DHAssessmentSimpleAggregationWrapper simpleAggregation:
                    switch (simpleAggregation.AggregationType)
                    {
                        case DHAssessmentSimpleAggregationType.Average:
                            var scoreWrappers = scores.Where(x => x.Scores.Any()).Select(x =>
                            {
                                switch (assessment.TargetEntityType)
                                {
                                    case DHAssessmentTargetEntityType.DataProduct:
                                        var dataProductId = x.EntityId ?? throw new InvalidOperationException("Data product id not found in entity payload!");
                                        var id = $"{controlId}_{computingJobId}_{dataProductId}";
                                        var score = new DHDataProductScoreWrapper
                                        {
                                            Id = id,
                                            ControlId = controlId,
                                            ControlGroupId = controlNode.GroupId,
                                            ComputingJobId = computingJobId,
                                            ScheduleRunId = scheduleRunId,
                                            Time = currentTime,
                                            Scores = x.Scores,
                                            AggregatedScore = x.Scores.Average(scoreUnit => scoreUnit.Score),
                                            DataProductDomainId = x.EntityPayload[DQOutputFields.BD_ID]?.ToString() ?? throw new InvalidOperationException("Data product domain id not found in entity payload!"),
                                            DataProductStatus = x.EntityPayload[DQOutputFields.DP_STATUS]?.ToString() ?? throw new InvalidOperationException("Data product status not found in entity payload!"),
                                            DataProductId = dataProductId,
                                            DataProductOwners = x.EntityPayload[DQOutputFields.DP_OWNER_IDS]?.ToString().Split(",") ?? throw new InvalidOperationException("Data product owner ids not found in entity payload!")
                                        };
                                        score.Validate();
                                        score.OnCreate(requestHeaderContext.ClientObjectId, id);
                                        return score;
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

        private async Task GenerateActionAsync(DHControlNodeWrapper control, DHAssessmentWrapper assessment, IEnumerable<DHRawScore> scores, string computingJobId, bool isRetry)
        {
            using (logger.LogElapsed($"Start to generate action by Controls, isRetry={isRetry}"))
            {
                if (!isRetry)
                {
                    List<DataHealthActionWrapper> actions = new List<DataHealthActionWrapper>();
                    var controlGroupId = control.GroupId;
                    var controlGroup = await dhControlRepository.GetByIdAsync(controlGroupId).ConfigureAwait(false);
                    var findingType = controlGroup?.Name ?? "";
                    var findingSubType = control.Name;
                    DataHealthActionTargetEntityType? targetEntityType = Enum.TryParse<DataHealthActionTargetEntityType>(assessment.TargetEntityType.ToString(), true, out var result) ? result : null;

                    if (targetEntityType == null)
                    {
                        logger.LogError($"Target entity type {assessment.TargetEntityType?.ToString()} is not supported in action center yet!");
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
                                var assessmentRuleId = scoreUnit.AssessmentRuleId;
                                var matchedAssessment = assessment.Rules.FirstOrDefault((rule) =>
                                {
                                    return rule.Id == assessmentRuleId;
                                });
                                if (matchedAssessment?.ActionProperties != null && matchedAssessment.ActionProperties?.Name != null)
                                {
                                    var actionName = matchedAssessment.ActionProperties.Name;
                                    var reason = matchedAssessment.ActionProperties.Reason;
                                    var recommendation = matchedAssessment.ActionProperties.Recommendation;

                                    var action = new DataHealthActionWrapper()
                                    {
                                        Status = scoreUnit.Score < 1 ? DataHealthActionStatus.NotStarted : DataHealthActionStatus.Resolved,
                                        Category = DataHealthActionCategory.HealthControl,
                                        Severity = matchedAssessment.ActionProperties?.Severity ?? DataHealthActionSeverity.Medium,
                                        FindingId = assessmentRuleId,
                                        FindingName = actionName,
                                        Reason = reason,
                                        Recommendation = recommendation,
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
                                else
                                {
                                    logger.LogError($"The ActionProperties or ActionProperties.Name is null, controlId: {control.Id}, jobId: {computingJobId}, matchedAssessmentId: {matchedAssessment?.Id}");
                                }
                            }
                        }
                        else
                        {
                            logger.LogError($"The businessDomainId or targetEntityId is null, controlId: {control.Id}, jobId: {computingJobId}");
                        }
                    }
                    logger.LogInformation($"The number of actions need to be stored: {actions.Count()}");

                    if (actions.Any())
                    {
                        await dHActionInternalService.BatchCreateOrUpdateActionsAsync(actions);
                    }
                }
            }
        }
    }
}
