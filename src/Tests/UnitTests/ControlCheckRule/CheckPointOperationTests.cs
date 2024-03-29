namespace UnitTests.ControlCheckRule;

using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

[TestClass]
public class CheckPointOperationTests
{
    [TestMethod]
    [Owner(Owners.RuleCheck)]
    public void CheckPointSourceCheck()
    {
        var allSourceTypes = Enum.GetValues<DHRuleSourceType>();

        var mappedCheckPoints = allSourceTypes.SelectMany(s => RuleOperatorMapping.GetAllowedCheckPoints(s));

        var allCheckPointValues = Enum.GetValues<DHCheckPoint>();

        var checkPointsNotMapped = allCheckPointValues.Where(x => !mappedCheckPoints.Contains(x)).ToList();

        Assert.IsTrue(checkPointsNotMapped.Count == 0, $"Checkpoints not mapped with a source type: {string.Join(", ", checkPointsNotMapped)}");
    }

    [TestMethod]
    [Owner(Owners.RuleCheck)]
    public void CheckPointOperationsCheck()
    {
        var allCheckPointValues = Enum.GetValues<DHCheckPoint>();

        foreach (var checkPoint in allCheckPointValues)
        {
            var allowedOperators = RuleOperatorMapping.GetAllowedOperators(checkPoint);

            Assert.IsTrue(allowedOperators.Count > 0, $"No operators are allowed for {checkPoint}");
        }
    }
}
