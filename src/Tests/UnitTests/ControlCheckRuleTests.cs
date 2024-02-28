namespace UnitTests;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ControlCheckRuleTests
{
    [TestMethod]
    public void SimpleRuleTest()
    {
        DHSimpleRuleWrapper rule = new DHSimpleRuleWrapper();
        Assert.IsNotNull(rule);
    }
}