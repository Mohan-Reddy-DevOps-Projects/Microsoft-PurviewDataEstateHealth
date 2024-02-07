namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHControlService(DHControlRepository dHControlRepository)
    {
        public async Task<IBatchResults<DHControlBaseWrapper>> ListControlsAsync()
        {
            var results = await dHControlRepository.GetAllAsync().ConfigureAwait(false);
            return new BatchResults<DHControlBaseWrapper>(results, results.Count());
        }

        public async Task<DHControlBaseWrapper> GetControlByIdAsync(string id)
        {
            var entity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Control.ToString(), id));
            }

            return entity;
        }

        public async Task<DHControlBaseWrapper> CreateControlAsync(DHControlBaseWrapper entity)
        {
            await dHControlRepository.AddAsync(entity).ConfigureAwait(false);
            return entity;
        }
    }
}
