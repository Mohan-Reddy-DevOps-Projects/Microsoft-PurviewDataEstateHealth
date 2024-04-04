namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using System.Threading.Tasks;

    public class DHDataEstateHealthService(
        DHDataEstateHealthRepository dHDataEstateHealthRepository,
        IDataEstateHealthRequestLogger logger)
    {
        public async Task DeprovisionForDEHAsync()
        {
            string[] containerNames = new string[] { "businessdomain", "dataasset", "dataproduct", "dataqualityfact", "datasubscription", "policyset", "relationship", "term" };
            var methodName = nameof(DeprovisionForDEHAsync);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForDEHAsync)}: Deprovision for Data Estate Health"))
            {
                foreach (string containerName in containerNames)
                {
                    logger.LogInformation($"{this.GetType().Name}#{methodName}, Starting deleting from {containerName} in CosmosDB during deprovisioning");
                    dHDataEstateHealthRepository.ContainerName = containerName;
                    await dHDataEstateHealthRepository.DeprovisionDEHAsync().ConfigureAwait(false);
                    logger.LogInformation($"{this.GetType().Name}#{methodName}, Finishing deleting from {containerName} in CosmosDB during deprovisioning");

                }
            }
        }
    }
}
