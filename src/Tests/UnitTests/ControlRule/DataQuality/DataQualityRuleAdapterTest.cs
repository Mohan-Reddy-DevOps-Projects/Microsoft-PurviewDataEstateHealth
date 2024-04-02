namespace UnitTests.ControlRule.DataQuality;

using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[TestClass]
public class DataQualityRuleAdapterTest
{
    [TestMethod]
    [Owner(Owners.DQAdapter)]
    public void TestNoErrorInParse()
    {
        var checkpointsInDevelopment = new HashSet<DHCheckPoint>() { };

        var fileName = $"CDMC.json";
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", fileName);
        var jsonStr = File.ReadAllText(fullPath);
        var removeComments = Regex.Replace(jsonStr, @"^\s*//.*$", "", RegexOptions.Multiline);  // removes comments like this

        var templates = JsonConvert.DeserializeObject<IList<ControlAssessmentTemplate>>(removeComments)!;

        var assessments = new List<DHAssessmentWrapper>();

        foreach (var template in templates)
        {
            var templateItems = template.Items;
            foreach (var templateItem in templateItems)
            {
                var control = DHControlBaseWrapper.Create(templateItem.Control);
                if (control.Status == DHControlStatus.Enabled)
                {
                    var assessment = DHAssessmentWrapper.Create(templateItem.Assessment);
                    if (assessment.TargetQualityType == DHAssessmentQualityType.MetadataQuality)
                    {
                        assessments.Add(assessment);
                    }
                }
            }
        }

        foreach (var assessment in assessments)
        {
            var adaptContext = new RuleAdapterContext(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                assessment,
                new List<string>());

            try
            {
                var convertedResult = DHAssessmentRulesAdapter.ToDqRules(adaptContext, assessment.Rules);
                Assert.IsNotNull(convertedResult);
            }
            catch (NotImplementedException ex)
            {
                if (!checkpointsInDevelopment.Any(c => ex.Message.Contains(c.ToString())))
                {
                    throw;
                }
            }
        }
    }
}
