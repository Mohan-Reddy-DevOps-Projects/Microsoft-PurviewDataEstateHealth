namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class DHProvisionService(
    DHControlService controlService,
    DHAssessmentService assessmentService)
{
    public async Task ProvisionControlTemplate(string templateName)
    {
        var templatePayload = this.GetTemplatePayload(TemplateType.ControlAssessment, templateName);

        var template = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(templatePayload) ?? [];

        foreach (var group in template)
        {
            var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
            var controlGroup = await controlService.CreateControlAsync(controlGroupWrapper).ConfigureAwait(false);

            foreach (var item in group.Items)
            {
                var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment ?? []);
                var assessment = await assessmentService.CreateAssessmentAsync(assessmentWrapper).ConfigureAwait(false);

                var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control ?? []);
                controlWrapper.AssessmentId = assessment.Id;
                controlWrapper.GroupId = controlGroup.Id;
                await controlService.CreateControlAsync(controlWrapper).ConfigureAwait(false);
            }
        }
    }

    private string GetTemplatePayload(TemplateType templateType, string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);

        var fileName = templateName.ToLower() switch
        {
            "cdmc" => "CDMC.json",
            _ => throw new EntityValidationException()
        };

        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", templateType.ToString(), fileName);

        var jsonStr = File.ReadAllText(fullPath);

        return jsonStr;
    }

    private enum TemplateType
    {
        ControlAssessment
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


