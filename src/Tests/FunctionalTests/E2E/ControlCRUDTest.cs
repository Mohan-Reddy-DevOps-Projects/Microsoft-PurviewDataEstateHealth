namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.E2E
{
    using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ControlCRUDTest : DEHE2EBase
    {
        public override TestFileInfo TestFileConfig => new()
        {
            FileFolder = "Controls",
        };

        //[TestMethod]
        //[Owner(Owners.Control)]
        //public async Task TestControlCRUDAndList()
        //{
        //    var response = await Client.Request<PagedResults<object>>("/controls", HttpMethod.Get).ConfigureAwait(false);
        //    Assert.IsTrue(response.Count >= 0);
        //}
    }
}
