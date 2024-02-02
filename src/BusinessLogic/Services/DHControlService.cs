namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using System.Threading.Tasks;

    public class DHControlService(DHControlRepository dHControlRepository)
    {
        public async Task<DHControlBaseWrapper?> GetControlByIdAsync(string id)
        {
            return await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task CreateControlAsync(DHControlBaseWrapper entity)
        {
            await dHControlRepository.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task RunScheduleJob(string id)
        {
            var control = await this.GetControlByIdAsync(id).ConfigureAwait(false);
            // TODO: submit DQ job
        }
    }
}
