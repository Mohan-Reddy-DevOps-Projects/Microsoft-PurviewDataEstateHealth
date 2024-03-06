namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class DHProvisionService(
    DHControlService controlService,
    DHAssessmentService assessmentService)
{
    public async Task ProvisionControlTemplate(string templateName)
    {
        if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
        {
            throw new EntityValidationException("Wrong template name");
        }

        var templatePayload = this.GetTemplatePayload(TemplateType.ControlAssessment, templateType);

        var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

        foreach (var group in template)
        {
            var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
            controlGroupWrapper.SystemTemplate = templateType.ToString();

            var controlGroup = await controlService.CreateControlAsync(controlGroupWrapper, isSystem: true).ConfigureAwait(false);

            foreach (var item in group.Items)
            {
                if (item.Control == null || item.Assessment == null)
                {
                    throw new EntityValidationException("Control and Assessment cannot be null");
                }

                var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                assessmentWrapper.SystemTemplate = templateType.ToString();
                var assessment = await assessmentService.CreateAssessmentAsync(assessmentWrapper, isSystem: true).ConfigureAwait(false);

                var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);
                controlWrapper.AssessmentId = assessment.Id;
                controlWrapper.GroupId = controlGroup.Id;
                controlWrapper.SystemTemplate = templateType.ToString();
                await controlService.CreateControlAsync(controlWrapper, isSystem: true).ConfigureAwait(false);
            }
        }
    }

    public void ValidateControlTemplate(string templateName)
    {
        if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
        {
            throw new EntityValidationException("Wrong template name");
        }

        var templatePayload = this.GetTemplatePayload(TemplateType.ControlAssessment, templateType);

        var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

        foreach (var group in template)
        {
            var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
            controlGroupWrapper.Validate();

            foreach (var item in group.Items)
            {
                if (item.Control == null || item.Assessment == null)
                {
                    throw new EntityValidationException("Control and Assessment cannot be null");
                }

                var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                assessmentWrapper.Validate();

                var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);
                controlWrapper.Validate();
            }
        }
    }

    public async Task CleanupControlTemplate(string templateName)
    {
        if (!Enum.TryParse<SystemTemplateNames>(templateName, true, out var templateType))
        {
            throw new EntityValidationException("Wrong template name");
        }

        var allControlsResponse = await controlService.ListControlsAsync().ConfigureAwait(false);

        var allTemplateControls = allControlsResponse.Results.Where((x) => x.SystemTemplate == templateType.ToString()).ToList();

        foreach (var control in allTemplateControls)
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

    private string GetTemplatePayload(TemplateType templateType, SystemTemplateNames templateName)
    {
        var fileName = $"{templateName}.json";

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateType.ToString(), fileName);

        var jsonStr = File.ReadAllText(fullPath);

        return jsonStr;
    }

    private enum TemplateType
    {
        ControlAssessment
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
