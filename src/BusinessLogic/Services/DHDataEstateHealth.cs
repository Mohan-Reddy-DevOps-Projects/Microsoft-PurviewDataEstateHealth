namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using System;
    using System.Threading.Tasks;

    public class DHDataEstateHealthService(
        DHDataEstateHealthRepository dHDataEstateHealthRepository,
        IDataEstateHealthRequestLogger logger)
    {
        public async Task DeprovisionForDEHAsync()
        {
            try
            {
                string[] containerNames = new string[] { "businessdomain", "dataasset", "dataproduct", "dataqualityfact", "dataqualityv2fact", "datasubscription", "policyset", "relationship", "term", "cde", "dcatalogall" };
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
            catch (Exception ex)
            {
                logger.LogError($"Error in {this.GetType().Name}#{nameof(DeprovisionForDEHAsync)} while deprovisioning for Data Estate Health", ex);
                throw;
            }
        }
    }
}
