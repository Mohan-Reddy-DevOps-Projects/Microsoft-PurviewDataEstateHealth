#nullable enable
namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;
    using System;
    using System.Threading.Tasks;

    public class DHControlService(DHControlNodeRepository dHControlNodeRepository)
    {
        public async Task<DHControlNode?> GetControlByIdAsync(Guid id)
        {
            return await dHControlNodeRepository.GetByIdAsync(id).ConfigureAwait(false);
        }

        public async Task CreateControlAsync(DHControlNode entity)
        {
            await dHControlNodeRepository.AddAsync(entity).ConfigureAwait(false);
        }
    }
}
