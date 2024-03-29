namespace UnitTests.ProvisionTemplate;

using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

[TestClass]
public class TemplateValidation
{
    [TestMethod]
    [Owner(Owners.ControlTemplate)]
    public void ValidateTemplates()
    {
        var allTemplateNames = Enum.GetValues<SystemTemplateNames>();

        foreach (var templateType in allTemplateNames)
        {
            var template = DHTemplateService.GetTemplatePayload(templateType);

            Assert.IsTrue(template.Count > 0, "Template is empty.");

            var templateEntityIds = new List<string>();

            foreach (var group in template)
            {
                var controlGroupWrapper = DHControlBaseWrapper.Create(group.ControlGroup);
                controlGroupWrapper.Validate();
                Assert.IsFalse(string.IsNullOrWhiteSpace(controlGroupWrapper.SystemTemplateEntityId), $"SystemTemplateEntityId cannot be null or empty in ControlGroup {controlGroupWrapper.Name}");
                templateEntityIds.Add(controlGroupWrapper.SystemTemplateEntityId);

                foreach (var item in group.Items)
                {
                    Assert.IsNotNull(item.Control, "Control cannot be null");
                    Assert.IsNotNull(item.Assessment, "Assessment cannot be null");

                    var assessmentWrapper = DHAssessmentWrapper.Create(item.Assessment);
                    assessmentWrapper.Validate();

                    Assert.IsFalse(string.IsNullOrWhiteSpace(assessmentWrapper.SystemTemplateEntityId), $"SystemTemplateEntityId cannot be null or empty in assessment {assessmentWrapper.Name}");

                    foreach (var rule in assessmentWrapper.Rules)
                    {
                        Assert.IsFalse(string.IsNullOrWhiteSpace(rule.Id), $"Id cannot be null or empty in assessment rule {JsonConvert.SerializeObject(rule)}");
                        templateEntityIds.Add(rule.Id);
                    }

                    templateEntityIds.Add(assessmentWrapper.SystemTemplateEntityId);

                    var controlWrapper = (DHControlNodeWrapper)DHControlBaseWrapper.Create(item.Control);
                    controlWrapper.Validate();
                    Assert.IsFalse(string.IsNullOrWhiteSpace(controlWrapper.SystemTemplateEntityId), $"SystemTemplateEntityId cannot be null or empty in control {controlWrapper.Name}");
                    templateEntityIds.Add(controlWrapper.SystemTemplateEntityId);
                }
            }

            var duplicateIds = templateEntityIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();

            Assert.IsFalse(duplicateIds.Any(), $"Duplicate SystemTemplateEntityId found: {string.Join(", ", duplicateIds)}");
        }

    }
}
