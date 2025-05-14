namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.BusinessLogic;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

public class DHTemplateService(
    DHControlService controlService,
    DHAssessmentService assessmentService,
    DHControlRepository controlRepository,
    DHAssessmentRepository assessmentRepository,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    IAccountExposureControlConfigProvider exposureControl
    )
{
    public async Task ProvisionControlTemplate(string templateName)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ProvisionControlTemplate)}: Provision for the template {templateName}."))
        {
            if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            var template = GetTemplatePayload(templateType);

            foreach (var group in template)
            {

                var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
                controlGroupWrapper.SystemTemplate = templateType.ToString();

                logger.LogInformation($"Provisioning ControlGroup {controlGroupWrapper.Name} (SystemEntityId {controlGroupWrapper.SystemTemplateEntityId}) for the template {templateName}");

                var controlGroup = await controlService.CreateControlAsync(controlGroupWrapper, isSystem: true).ConfigureAwait(false);

                foreach (var item in group.Items)
                {
                    if (item.Control == null || item.Assessment == null)
                    {
                        throw new EntityValidationException("Control and Assessment cannot be null");
                    }

                    var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                    assessmentWrapper.SystemTemplate = templateType.ToString();

                    logger.LogInformation($"Provisioning Assessment {assessmentWrapper.Name} (SystemEntityId {assessmentWrapper.SystemTemplateEntityId}) for the template {templateName}");

                    var assessment = await assessmentService.CreateAssessmentAsync(assessmentWrapper, isSystem: true).ConfigureAwait(false);

                    var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);
                    controlWrapper.AssessmentId = assessment.Id;
                    controlWrapper.GroupId = controlGroup.Id;
                    controlWrapper.SystemTemplate = templateType.ToString();
                    
                    // Check if Business OKRs alignment is enabled from exposure control
                    this.UpdateControlStatusBasedOnECFlag(controlWrapper);

                    logger.LogInformation($"Provisioning Control Node {controlWrapper.Name} (SystemEntityId {controlWrapper.SystemTemplateEntityId}) for the template {templateName}");

                    await controlService.CreateControlAsync(controlWrapper, isSystem: true).ConfigureAwait(false);
                }
            }
        }
    }

    public async Task<DHControlBaseWrapper> ResetControlByIdAsync(string controlId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ResetControlByIdAsync)}: Reset control by id {controlId}."))
        {
            var entity = await controlService.GetControlByIdAsync(controlId).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Control.ToString(), controlId));
            }

            if (entity.SystemTemplate == null)
            {
                throw new EntityValidationException(string.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageNotCreatedWithTemplate, EntityCategory.Control.ToString(), controlId));
            }

            if (!Enum.TryParse<SystemTemplateNames>(entity.SystemTemplate, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            logger.LogInformation($"Resetting control ID {controlId} (SystemEntityId {entity.SystemTemplateEntityId}) for the template {templateType}");

            var template = GetTemplatePayload(templateType);

            switch (entity.Type)
            {
                case DHControlBaseWrapperDerivedTypes.Group:
                    var allControlGroupsInTemplate = template.Select(x => DHControlBaseWrapper.Create(x.ControlGroup)).ToList();

                    var templateGroup = allControlGroupsInTemplate.FirstOrDefault(x => x.SystemTemplateEntityId == entity.SystemTemplateEntityId);
                    if (templateGroup == null)
                    {
                        throw new EntityValidationException("Group not found in template");
                    }

                    templateGroup.Id = entity.Id;
                    if (entity.Contacts != null)
                    {
                        templateGroup.Contacts = entity.Contacts;
                    }

                    return await controlService.UpdateControlByIdAsync(controlId, templateGroup, true).ConfigureAwait(false);
                case DHControlBaseWrapperDerivedTypes.Node:
                    var nodeEntity = (DHControlNodeWrapper)entity;

                    var allControlItemsInTemplate = template.SelectMany(x => x.Items).Select(x => (DHControlNodeWrapper)DHControlBaseWrapper.Create(x.Control)).ToList();

                    var templateNode = allControlItemsInTemplate.FirstOrDefault(x => x.SystemTemplateEntityId == entity.SystemTemplateEntityId);
                    if (templateNode == null)
                    {
                        throw new EntityValidationException("Node not found in template");
                    }

                    templateNode.Id = nodeEntity.Id;
                    templateNode.AssessmentId = nodeEntity.AssessmentId;
                    templateNode.GroupId = nodeEntity.GroupId;
                    if (nodeEntity.Contacts != null)
                    {
                        templateNode.Contacts = nodeEntity.Contacts;
                    }
                    
                    // Check if Business OKRs alignment is enabled from exposure control
                    this.UpdateControlStatusBasedOnECFlag(templateNode);
                    
                    return await controlService.UpdateControlByIdAsync(controlId, templateNode, true).ConfigureAwait(false);
                default:
                    throw new EntityValidationException("Wrong control type");
            }
        }
    }

    public async Task<DHAssessmentWrapper> ResetAssessmentByIdAsync(string assessmentId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ResetAssessmentByIdAsync)}: Reset assessment by id {assessmentId}."))
        {
            var entity = await assessmentService.GetAssessmentByIdAsync(assessmentId).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Assessment.ToString(), assessmentId));
            }

            if (entity.SystemTemplate == null)
            {
                throw new EntityValidationException(string.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageNotCreatedWithTemplate, EntityCategory.Assessment.ToString(), assessmentId));
            }

            if (!Enum.TryParse<SystemTemplateNames>(entity.SystemTemplate, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            logger.LogInformation($"Resetting assessment ID {assessmentId} (SystemEntityId {entity.SystemTemplateEntityId}) for the template {templateType}");

            var template = GetTemplatePayload(templateType);

            var allAssessmentsInTemplate = template.SelectMany(x => x.Items).Select(x => DHAssessmentWrapper.Create(x.Assessment)).ToList();

            var templateAssessment = allAssessmentsInTemplate.FirstOrDefault(x => x.SystemTemplateEntityId == entity.SystemTemplateEntityId);

            if (templateAssessment == null)
            {
                throw new EntityValidationException("Assessment not found in template");
            }
            templateAssessment.Id = entity.Id;

            return await assessmentService.UpdateAssessmentByIdAsync(assessmentId, templateAssessment, true).ConfigureAwait(false);
        }
    }

    public async Task ResetControlTemplate(string templateName)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ResetControlTemplate)}: Reset for template {templateName}."))
        {
            if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }
            var template = GetTemplatePayload(templateType);

            var allExistingTemplateControlGroups = await controlRepository.QueryControlGroupsAsync(new TemplateFilters() { TemplateName = templateType.ToString() }).ConfigureAwait(false);

            var allExistingTemplateControlNodes = await controlRepository.QueryControlNodesAsync(new ControlNodeFilters() { TemplateName = templateType.ToString() }).ConfigureAwait(false);

            var allExistingTemplateAssessments = await assessmentRepository.QueryAssessmentsAsync(new TemplateFilters() { TemplateName = templateType.ToString() }).ConfigureAwait(false);

            logger.LogInformation($"Find exising resources. {allExistingTemplateControlGroups.Count()} Control Groups, {allExistingTemplateControlNodes.Count()} Control Nodes, {allExistingTemplateAssessments.Count()} Assessments.");
            List<string> controlIds = [];
            List<string> assessmentIds = [];

            foreach (var group in template)
            {
                var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);

                var existControlGroup = allExistingTemplateControlGroups.FirstOrDefault(x => x.SystemTemplateEntityId == controlGroupWrapper.SystemTemplateEntityId);

                // Update control group status based on EC flags
                this.UpdateControlGroupStatusBasedOnECFlag(controlGroupWrapper);

                DHControlBaseWrapper controlGroup;

                if (existControlGroup == null)
                {
                    logger.LogInformation($"Exist control group not found. Create ControlGroup {controlGroupWrapper.Name} (SystemEntityId {controlGroupWrapper.SystemTemplateEntityId}) for the template {templateName}");

                    controlGroupWrapper.SystemTemplate = templateType.ToString();

                    controlGroup = await controlService.CreateControlAsync(controlGroupWrapper, isSystem: true).ConfigureAwait(false);
                }
                else
                {
                    logger.LogInformation($"Found exist control group. Update ControlGroup {controlGroupWrapper.Name} with ID {existControlGroup.Id} (SystemEntityId {controlGroupWrapper.SystemTemplateEntityId}) for the template {templateName}");

                    controlGroupWrapper.Id = existControlGroup.Id;
                    if (existControlGroup.Contacts != null)
                    {
                        controlGroupWrapper.Contacts = existControlGroup.Contacts;
                    }

                    controlGroup = await controlService.UpdateControlByIdAsync(existControlGroup.Id, controlGroupWrapper, true).ConfigureAwait(false);
                }
                controlIds.Add(controlGroup.Id);

                foreach (var item in group.Items)
                {
                    if (item.Control == null || item.Assessment == null)
                    {
                        throw new EntityValidationException("Control and Assessment cannot be null");
                    }

                    // Reset assessment

                    var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                    var existAssessment = allExistingTemplateAssessments.FirstOrDefault(x => x.SystemTemplateEntityId == assessmentWrapper.SystemTemplateEntityId);

                    DHAssessmentWrapper assessment;

                    if (existAssessment == null)
                    {
                        logger.LogInformation($"Exist assessment not found. Create Assessment {assessmentWrapper.Name} (SystemEntityId {assessmentWrapper.SystemTemplateEntityId}) for the template {templateName}");

                        assessmentWrapper.SystemTemplate = templateType.ToString();

                        assessment = await assessmentService.CreateAssessmentAsync(assessmentWrapper, isSystem: true).ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogInformation($"Found exist assessment. Update Assessment {assessmentWrapper.Name} with ID {existAssessment.Id} (SystemEntityId {assessmentWrapper.SystemTemplateEntityId}) for the template {templateName}");

                        assessmentWrapper.Id = existAssessment.Id;
                        assessment = await assessmentService.UpdateAssessmentByIdAsync(existAssessment.Id, assessmentWrapper, true).ConfigureAwait(false);
                    }

                    assessmentIds.Add(assessment.Id);

                    // Reset control

                    var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);

                    var existControl = allExistingTemplateControlNodes.FirstOrDefault(x => x.SystemTemplateEntityId == controlWrapper.SystemTemplateEntityId);
                    DHControlBaseWrapper controlNode;

                    controlWrapper.AssessmentId = assessment.Id;
                    controlWrapper.GroupId = controlGroup.Id;
                    controlWrapper.SystemTemplate = templateType.ToString();
                    
                    // Check if Business OKRs alignment is enabled from exposure control
                    this.UpdateControlStatusBasedOnECFlag(controlWrapper);

                    if (existControl == null)
                    {
                        logger.LogInformation($"Exist control node not found. Create ControlNode {controlWrapper.Name} (SystemEntityId {controlWrapper.SystemTemplateEntityId}) for the template {templateName}");

                        controlWrapper.SystemTemplate = templateType.ToString();

                        controlNode = await controlService.CreateControlAsync(controlWrapper, isSystem: true).ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogInformation($"Found exist control node. Update ControlNode {controlWrapper.Name} with ID {existControl.Id} (SystemEntityId {controlWrapper.SystemTemplateEntityId}) for the template {templateName}");

                        controlWrapper.Id = existControl.Id;
                        if (existControl.Contacts != null)
                        {
                            controlWrapper.Contacts = existControl.Contacts;
                        }

                        controlNode = await controlService.UpdateControlByIdAsync(existControl.Id, controlWrapper, true).ConfigureAwait(false);
                    }

                    controlIds.Add(controlNode.Id);
                }
            }

            var allControlNodesNotInTemplate = allExistingTemplateControlNodes.Where(x => !controlIds.Contains(x.Id)).Select(x => x.Id).ToList();

            foreach (var controlId in allControlNodesNotInTemplate)
            {
                logger.LogInformation($"Delete control node {controlId} that is removed from the template.");

                await controlService.DeleteControlByIdAsync(controlId).ConfigureAwait(false);
            }

            var allControlGroupsNotInTemplate = allExistingTemplateControlGroups.Where(x => !controlIds.Contains(x.Id)).Select(x => x.Id).ToList();

            foreach (var controlId in allControlGroupsNotInTemplate)
            {
                logger.LogInformation($"Delete control group {controlId} that is removed from the template.");

                await controlService.DeleteControlByIdAsync(controlId).ConfigureAwait(false);
            }

            var allAssessmentsNotInTemplate = allExistingTemplateAssessments.Where(x => !assessmentIds.Contains(x.Id)).Select(x => x.Id).ToList();

            foreach (var assessmentId in allAssessmentsNotInTemplate)
            {
                logger.LogInformation($"Delete assessment {assessmentId} that is removed from the template.");

                await assessmentService.DeleteAssessmentByIdAsync(assessmentId).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Updates the control status based on exposure control flags.
    /// </summary>
    /// <param name="controlWrapper">The control wrapper to update.</param>
    private void UpdateControlStatusBasedOnECFlag(DHControlBaseWrapper controlWrapper)
    {
        if (controlWrapper is DHControlNodeWrapper controlNodeWrapper)
        {
            var accountId = requestHeaderContext.AccountObjectId.ToString();
            var tenantId = requestHeaderContext.TenantId.ToString();

            // Check if the control is "Business OKRs alignment" and update status based on EC flag
            if (string.Equals(controlNodeWrapper.Name, DHControlConstants.BusinessOKRsAlignment, StringComparison.OrdinalIgnoreCase))
            {
                bool isEnabled = exposureControl.IsDEHBusinessOKRsAlignmentEnabled(accountId, string.Empty, tenantId);
                controlNodeWrapper.Status = isEnabled ? DHControlStatus.Enabled : DHControlStatus.InDevelopment;
                logger.LogInformation($"Set control {controlNodeWrapper.Name} status to {controlNodeWrapper.Status} based on EC flag (IsEnabled: {isEnabled})");
            }
            // Check if the control is "Critical data identification" and update status based on EC flag
            else if (string.Equals(controlNodeWrapper.Name, DHControlConstants.CriticalDataIdentification, StringComparison.OrdinalIgnoreCase))
            {
                bool isEnabled = exposureControl.IsDEHCriticalDataIdentificationEnabled(accountId, string.Empty, tenantId);
                controlNodeWrapper.Status = isEnabled ? DHControlStatus.Enabled : DHControlStatus.InDevelopment;
                logger.LogInformation($"Set control {controlNodeWrapper.Name} status to {controlNodeWrapper.Status} based on EC flag (IsEnabled: {isEnabled})");
            }
        }
    }

    /// <summary>
    /// Updates the control group status based on exposure control flags.
    /// </summary>
    /// <param name="controlGroupWrapper">The control group wrapper to update.</param>
    private void UpdateControlGroupStatusBasedOnECFlag(DHControlBaseWrapper controlGroupWrapper)
    {
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();

        // Check if the control group is "Value Creation" and update status based on Business OKRs alignment EC flag
        if (string.Equals(controlGroupWrapper.Name, DHControlConstants.ValueCreation, StringComparison.OrdinalIgnoreCase) &&
            controlGroupWrapper.Status == DHControlStatus.InDevelopment)
        {
            bool isEnabled = exposureControl.IsDEHBusinessOKRsAlignmentEnabled(accountId, string.Empty, tenantId);
            if (isEnabled)
            {
                controlGroupWrapper.Status = DHControlStatus.Enabled;
                logger.LogInformation($"Set control group {controlGroupWrapper.Name} status to {controlGroupWrapper.Status} based on Business OKRs alignment EC flag");
            }
        }
    }

    public static IList<ControlAssessmentTemplate> GetTemplatePayload(SystemTemplateNames templateName)
    {
        var fileName = $"{templateName}.json";

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", fileName);

        var jsonStr = File.ReadAllText(fullPath);

        var removeComments = Regex.Replace(jsonStr, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes comments like this

        return JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(removeComments) ?? [];
    }
}

public enum SystemTemplateNames
{
    CDMC
}

public class ControlAssessmentTemplate
{
    [JsonProperty("controlGroup", NullValueHandling = NullValueHandling.Ignore)]
    public JObject ControlGroup { get; set; } = [];

    [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
    public IList<ControlAssessmentItemTemplate> Items { get; set; } = [];

}

public class ControlAssessmentItemTemplate
{
    [JsonProperty("control", NullValueHandling = NullValueHandling.Ignore)]
    public JObject Control { get; set; } = [];

    [JsonProperty("assessment", NullValueHandling = NullValueHandling.Ignore)]
    public JObject Assessment { get; set; } = [];
}