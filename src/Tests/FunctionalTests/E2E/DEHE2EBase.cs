namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.E2E
{
    using Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DEHE2EBase
    {
        public virtual TestFileInfo TestFileConfig { get; }

        public static TestClientBase Client { get; private set; }

        [AssemblyInitialize()]
        public static void InitializeClient(TestContext context)
        {
            Client = new TestClientBase();
            Client.InitializeClient();
        }

        [AssemblyCleanup()]
        public static void Cleanup()
        {
            // TODO
        }

        public string GetEntityJson(string fileName, string domainId = null)
        {
            if (this.TestFileConfig == null)
            {
                throw new InvalidOperationException("TestFileConfig is not set.");
            }

            var json = File.ReadAllText(Path.Combine(TestUtils.GetWrappersTestFileFolder(), this.TestFileConfig.FileFolder, fileName));

            Console.WriteLine(this.TestFileConfig.CommonDomainId);

            return json.Replace("%%DomainID%%", domainId ?? this.TestFileConfig.CommonDomainId);
        }
    }

    public class TestFileInfo
    {
        public string FileFolder { get; set; }

        public string CommonDomainId { get; set; }
    }
}
