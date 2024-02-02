namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using System.Threading.Tasks;

    public class DHControlService(DHControlNodeRepository dHControlNodeRepository)
    {
        public async Task<DHControlNodeWrapper?> GetControlByIdAsync(string id)
        {
            return await dHControlNodeRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task CreateControlAsync(DHControlNodeWrapper entity)
        {
            await dHControlNodeRepository.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task RunScheduleJob(string id)
        {
            var control = await this.GetControlByIdAsync(id).ConfigureAwait(false);
            // TODO: submit DQ job
        }
    }
}
