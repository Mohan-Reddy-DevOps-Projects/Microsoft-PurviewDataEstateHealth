namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHStorageConfigService
    {
        readonly private DHStorageConfigRepository dhStorageConfigRepository;
        readonly private IRequestHeaderContext requestHeaderContext;
        readonly private IDataEstateHealthRequestLogger logger;
        readonly private PurviewMITokenClient purviewMITokenClient;
        readonly private FabricOnelakeClient fabricOnelakeClient;
        readonly private DHStorageConnectionTestInternalService dhStorageConnectionTestInternalService;

        public DHStorageConfigService(
            DHStorageConfigRepository dhStorageConfigRepository,
            IRequestHeaderContext requestHeaderContext,
            PurviewMITokenClientFactory purviewMITokenClientFactory,
            FabricOnelakeClientFactory fabricOnelakeClientFactory,
            IDataEstateHealthRequestLogger logger,
            DHStorageConnectionTestInternalService dhStorageConnectionTestInternalService)
        {
            this.dhStorageConfigRepository = dhStorageConfigRepository;
            this.requestHeaderContext = requestHeaderContext;
            this.logger = logger;
            this.purviewMITokenClient = purviewMITokenClientFactory.GetClient();
            this.fabricOnelakeClient = fabricOnelakeClientFactory.GetClient();
            this.dhStorageConnectionTestInternalService = dhStorageConnectionTestInternalService;
        }

        public async Task<DHStorageConfigBaseWrapper> GetStorageConfig(string accountId)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(GetStorageConfig)}: Read storage config for account {accountId}."))
            {
                var entities = await this.dhStorageConfigRepository.GetAllAsync().ConfigureAwait(false);
                if (entities.Count() == 0)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StorageConfig.ToString(), accountId.ToString()));
                }
                return entities.First(); ;
            }
        }

        public async Task<DHStorageConfigBaseWrapper> UpdateStorageConfig(DHStorageConfigBaseWrapper entity, string accountId)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateStorageConfig)}: Update storage config."))
            {
                entity.Validate();

                try
                {
                    var existingEntity = await this.GetStorageConfig(accountId).ConfigureAwait(false);
                    this.logger.LogInformation("Existing storage config found. Update existing entity.");
                    entity.OnUpdate(existingEntity, accountId);
                    await this.dhStorageConfigRepository.UpdateAsync(entity).ConfigureAwait(false);
                }
                catch (EntityNotFoundException)
                {
                    this.logger.LogInformation("Existing storage config not found. Create new entity.");
                    entity.OnCreate(accountId);
                    await this.dhStorageConfigRepository.AddAsync(entity).ConfigureAwait(false);
                }

                return entity;
            }
        }

        public async Task<string> GetPurviewMIToken(string accountId)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(GetPurviewMIToken)}: Get purview MI."))
            {

                var token = string.Empty;
                try
                {
                    token = await this.purviewMITokenClient.GetMIToken(accountId, "azurestorage").ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Fail to get MI token.", ex);
                }
                return token;
            }
        }

        public async Task TestStorageConnection(DHStorageConfigBaseWrapper entity, string accountId)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(TestStorageConnection)}: Test storage connection."))
            {
                entity.Validate();

                var token = await this.GetPurviewMIToken(accountId);
                if (token == String.Empty)
                {
                    throw new Exception("Failed to get MI token");
                }

                try
                {
                    await this.dhStorageConnectionTestInternalService.TestConnection(entity, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Test connection failed.", ex);
                    throw new Exception("Test connection failed");
                }
            }
        }
    }
}
