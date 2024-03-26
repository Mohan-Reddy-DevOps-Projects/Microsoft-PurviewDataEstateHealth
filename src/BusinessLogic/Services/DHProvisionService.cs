namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class DHProvisionService(
    DHControlService controlService,
    DHAssessmentService assessmentService,
    DHStatusPaletteService statusPaletteService,
    DHActionService actionService,
    DHScheduleInternalService scheduleInternalService,
    DHControlRepository controlRepository,
    DHAssessmentRepository assessmentRepository,
    IRequestHeaderContext requestHeaderContext,
    IDataEstateHealthRequestLogger logger
    )
{
    public async Task ProvisionAccount(Guid tenantId, Guid accountId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ProvisionAccount)}"))
        {
            requestHeaderContext.TenantId = tenantId;
            requestHeaderContext.AccountObjectId = accountId;

            var allControls = await controlService.ListControlsAsync().ConfigureAwait(false);

            var controlTemplates = new List<string>() { SystemTemplateNames.CDMC.ToString() };

            foreach (var template in controlTemplates)
            {
                var templateProvisioned = allControls.Results.Any(x => x.SystemTemplate == template);

                if (!templateProvisioned)
                {
                    await this.ProvisionControlTemplate(template).ConfigureAwait(false);

                    logger.LogInformation($"Template {template} provisioned");
                }
                else
                {
                    logger.LogInformation($"Template {template} already provisioned, skip for provision.");
                }
            }
        }
    }

    public async Task DeprovisionAccount()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionAccount)}"))
        {
            List<Task> tasks = [
                controlService.DeprovisionForControlsAsync(),
                assessmentService.DeprovisionForAssessmentsAsync(),
                statusPaletteService.DeprovisionForStatusPalettesAsync(),
                scheduleInternalService.DeprovisionForSchedulesAsync(),
                actionService.DeprovisionForActionsAsync()
            ];

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    public async Task DeprovisionAccount(Guid tenantId, Guid accountId)
    {
        requestHeaderContext.TenantId = tenantId;
        requestHeaderContext.AccountObjectId = accountId;

        await this.DeprovisionAccount().ConfigureAwait(false);
    }

    public async Task ProvisionControlTemplate(string templateName)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ProvisionControlTemplate)}: Provision for the template {templateName}."))
        {
            if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            var templatePayload = this.GetTemplatePayload(templateType);

            var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

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

                    logger.LogInformation($"Provisioning Control Node {controlWrapper.Name} (SystemEntityId {controlWrapper.SystemTemplateEntityId}) for the template {templateName}");

                    await controlService.CreateControlAsync(controlWrapper, isSystem: true).ConfigureAwait(false);
                }
            }
        }
    }

    public void ValidateControlTemplate(string templateName)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ValidateControlTemplate)}: Validate for the template {templateName}."))
        {
            if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            var templatePayload = this.GetTemplatePayload(templateType);

            var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

            var templateEntityIds = new List<string>();

            foreach (var group in template)
            {
                var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
                controlGroupWrapper.Validate();
                Ensure.IsNotNullOrWhitespace(controlGroupWrapper.SystemTemplateEntityId, $"SystemTemplateEntityId cannot be null or empty in ControlGroup {controlGroupWrapper.Name}");
                templateEntityIds.Add(controlGroupWrapper.SystemTemplateEntityId);

                foreach (var item in group.Items)
                {
                    if (item.Control == null || item.Assessment == null)
                    {
                        throw new EntityValidationException("Control and Assessment cannot be null");
                    }

                    var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                    assessmentWrapper.Validate();
                    Ensure.IsNotNullOrWhitespace(assessmentWrapper.SystemTemplateEntityId, $"SystemTemplateEntityId cannot be null or empty in assessment {assessmentWrapper.Name}");

                    foreach (var rule in assessmentWrapper.Rules)
                    {
                        Ensure.IsNotNullOrWhitespace(rule.Id, $"Id cannot be null or empty in assessment rule {JsonConvert.SerializeObject(rule)}");
                        templateEntityIds.Add(rule.Id);
                    }

                    templateEntityIds.Add(assessmentWrapper.SystemTemplateEntityId);

                    var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);
                    controlWrapper.Validate();
                    Ensure.IsNotNullOrWhitespace(controlWrapper.SystemTemplateEntityId, $"SystemTemplateEntityId cannot be null or empty in control {controlWrapper.Name}");
                    templateEntityIds.Add(controlWrapper.SystemTemplateEntityId);
                }
            }

            var duplicateIds = templateEntityIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            if (duplicateIds.Any())
            {
                throw new EntityValidationException($"Duplicate SystemTemplateEntityId found: {string.Join(", ", duplicateIds)}");
            }
        }
    }

    public async Task CleanupControlTemplate(string templateName)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CleanupControlTemplate)}: Cleanup for the template {templateName}."))
        {
            if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
            {
                throw new EntityValidationException("Wrong template name");
            }

            var allControlsResponse = await controlService.ListControlsAsync().ConfigureAwait(false);

            var allTemplateControls = allControlsResponse.Results.Where((x) => x.SystemTemplate == templateType.ToString()).ToList();

            var groupControls = allTemplateControls.Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Group).ToList();

            var nodeControls = allTemplateControls.Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Node).ToList();

            foreach (var control in nodeControls)
            {
                await controlService.DeleteControlByIdAsync(control.Id, true).ConfigureAwait(false);
            }

            foreach (var control in groupControls)
            {
                await controlService.DeleteControlByIdAsync(control.Id).ConfigureAwait(false);
            }

            var allAssessmentsResponse = await assessmentService.ListAssessmentsAsync().ConfigureAwait(false);

            var allTemplateAssessments = allAssessmentsResponse.Results.Where((x) => x.SystemTemplate == templateType.ToString()).ToList();

            foreach (var assessment in allTemplateAssessments)
            {
                await assessmentService.DeleteAssessmentByIdAsync(assessment.Id).ConfigureAwait(false);
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

            var templatePayload = this.GetTemplatePayload(templateType);

            var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

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

            var templatePayload = this.GetTemplatePayload(templateType);

            var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

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

            var templatePayload = this.GetTemplatePayload(templateType);

            var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

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

    private string GetTemplatePayload(SystemTemplateNames templateName)
    {
        var fileName = $"{templateName}.json";

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", fileName);

        var jsonStr = File.ReadAllText(fullPath);

        var removeComments = Regex.Replace(jsonStr, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes comments like this

        return removeComments;
    }

    private enum SystemTemplateNames
    {
        CDMC
    }
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
