namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
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

        public async Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryScoreGroupByControlGroup(IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
        {
            var dqGroups = await dhControlRepository.QueryControlGroupsAsync(new TemplateFilters
            {
                TemplateEntityIds = [DHModelConstants.CONTROL_TEMPLATE_ID_DQGROUP]
            }).ConfigureAwait(false);
            return await dhScoreRepository.QueryScoreGroupByControlGroup(dqGroups.Select(x => x.Id), controlGroupIds, domainIds, recordLatestCounts, start, end, status);
        }

        public async Task DeprovisionForScoresAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForScoresAsync)}: Deprovision scores"))
            {
                try
                {
                    await dhScoreRepository.DeprovisionAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError($"Error in {this.GetType().Name}#{nameof(DeprovisionForScoresAsync)} while deprovisioning for scores", ex);
                    throw;
                }
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
                            var scoreWrappers = scores.Where(x => x.Scores.Any()).Select<DHRawScore, DHScoreBaseWrapper>(x =>
                                CreateScoreWrapperForAggregation(x, assessment.TargetEntityType, controlId, controlNode.GroupId, computingJobId, scheduleRunId, currentTime, requestHeaderContext.ClientObjectId)
                            ).ToList();
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

        private static DHScoreBaseWrapper CreateScoreWrapperForAggregation(
            DHRawScore rawScore,
            DHAssessmentTargetEntityType? targetEntityType,
            string controlId,
            string controlGroupId,
            string computingJobId,
            string scheduleRunId,
            DateTime currentTime,
            string clientObjectId)
        {
            string entityId = rawScore.EntityId ?? throw new InvalidOperationException($"{targetEntityType} id not found in entity payload!");
            string id = $"{controlId}_{computingJobId}_{entityId}";

            DHScoreBaseWrapper scoreWrapper = targetEntityType switch
            {
                DHAssessmentTargetEntityType.DataProduct => new DHDataProductScoreWrapper
                {
                    Id = id,
                    DataProductDomainId = rawScore.EntityPayload[DqOutputFields.BD_ID]?.ToString() ?? throw new InvalidOperationException("Data product domain id not found in entity payload!"),
                    DataProductStatus = rawScore.EntityPayload[DqOutputFields.DP_STATUS]?.ToString() ?? throw new InvalidOperationException("Data product status not found in entity payload!"),
                    DataProductId = entityId,
                    DataProductOwners = rawScore.EntityPayload[DqOutputFields.DP_OWNER_IDS]?.ToString()?.Split(",") ?? throw new InvalidOperationException("Data product owner ids not found in entity payload!")
                },
                DHAssessmentTargetEntityType.BusinessDomain => new DhBusinessDomainScoreWrapper
                {
                    Id = id,
                    BusinessDomainId = entityId,
                    BusinessDomainCriticalDataElementCount = (int?)rawScore.EntityPayload[DqOutputFields.keyBdCdeCount]
                },
                _ => throw new NotImplementedException($"Target entity type {targetEntityType} not supported yet!")
            };

            // Set common properties
            scoreWrapper.ControlId = controlId;
            scoreWrapper.ControlGroupId = controlGroupId;
            scoreWrapper.ComputingJobId = computingJobId;
            scoreWrapper.ScheduleRunId = scheduleRunId;
            scoreWrapper.Time = currentTime;
            scoreWrapper.Scores = rawScore.Scores;
            scoreWrapper.AggregatedScore = rawScore.Scores.Average(scoreUnit => scoreUnit.Score);

            // Validate and initialize
            scoreWrapper.Validate();
            scoreWrapper.OnCreate(clientObjectId, id);

            return scoreWrapper;
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
                        string? targetEntityId;
                        string? businessDomainId;

                        // Handle different entity types
                        if (assessment.TargetEntityType == DHAssessmentTargetEntityType.BusinessDomain)
                        {
                            // For BusinessDomain entities
                            targetEntityId = (string?)score.EntityPayload[DqOutputFields.BD_ID];
                            businessDomainId = targetEntityId; // BusinessDomain ID is both target and domain
                            // BusinessDomain entities don't have owners in our current model
                        }
                        else
                        {
                            // For DataProduct entities (existing logic)
                            var owners = (string?)score.EntityPayload[DqOutputFields.DP_OWNER_IDS] ?? "";
                            if (!string.IsNullOrEmpty(owners))
                            {
                                entityOwners.AddRange(owners.Split(","));
                            }
                            targetEntityId = (string?)score.EntityPayload[DqOutputFields.DP_ID];
                            businessDomainId = (string?)score.EntityPayload[DqOutputFields.BD_ID];
                        }

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
