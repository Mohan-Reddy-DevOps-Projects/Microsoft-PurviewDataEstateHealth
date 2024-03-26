namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Configuration
{
    using Microsoft.Extensions.Configuration;

    internal class TestConfigurationHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                  .AddJsonFile("appsettings.tests.json", optional: true)
                .Build();
        }

        public static TestConfiguration GetApplicationConfiguration()
        {
            var configuration = new TestConfiguration();
            var iConfig = GetIConfigurationRoot();

            iConfig
                .Bind(configuration);

            return configuration;
        }
    }
}
