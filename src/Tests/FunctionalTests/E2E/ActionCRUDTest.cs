namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.E2E
{
    using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ActionCRUDTest : DEHE2EBase
    {
        public override TestFileInfo TestFileConfig => new()
        {
            FileFolder = "Actions",
        };

        //[TestMethod]
        //[Owner(Owners.Action)]
        //public async Task TestActionCRUDAndList()
        //{
        //    var actionQuery = new ActionQueryRequest();
        //    var response = await Client.Request<PagedResults<object>>("/actions/query", HttpMethod.Post, actionQuery).ConfigureAwait(false);
        //    Assert.AreEqual(response.Count, 0);
        //}
    }
}
