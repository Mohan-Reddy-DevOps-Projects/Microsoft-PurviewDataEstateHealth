namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHStorageConfigService(
        DHStorageConfigRepository dhStorageConfigRepository,
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger logger)
    {
        public async Task<DHStorageConfigBaseWrapper> GetStorageConfig()
        {
            var accountId = requestHeaderContext.AccountObjectId;
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GetStorageConfig)}: Read storage config for account {accountId}."))
            {
                var entities = await dhStorageConfigRepository.GetAllAsync().ConfigureAwait(false);
                if (entities.Count() == 0)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StorageConfig.ToString(), accountId.ToString()));
                }
                return entities.First(); ;
            }
        }

        public async Task<DHStorageConfigBaseWrapper> UpdateStorageConfig(DHStorageConfigBaseWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateStorageConfig)}: Update storage config."))
            {
                entity.Validate();

                try
                {
                    var existingEntity = await this.GetStorageConfig().ConfigureAwait(false);
                    logger.LogInformation("Existing storage config found. Update existing entity.");
                    entity.OnUpdate(existingEntity, requestHeaderContext.AccountObjectId.ToString());
                    await dhStorageConfigRepository.UpdateAsync(entity).ConfigureAwait(false);
                }
                catch (EntityNotFoundException)
                {
                    logger.LogInformation("Existing storage config not found. Create new entity.");
                    entity.OnCreate(requestHeaderContext.AccountObjectId.ToString());
                    await dhStorageConfigRepository.AddAsync(entity).ConfigureAwait(false);
                }

                return entity;
            }
        }
    }
}
