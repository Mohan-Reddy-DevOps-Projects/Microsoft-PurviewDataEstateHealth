// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System;
    using System.Threading.Tasks;

    public class DHStorageConnectionTestInternalService
    {
        readonly private DHStorageConfigRepository dhStorageConfigRepository;
        readonly private IRequestHeaderContext requestHeaderContext;
        readonly private IDataEstateHealthRequestLogger logger;
        readonly private PurviewMITokenClient purviewMITokenClient;
        readonly private FabricOnelakeClient fabricOnelakeClient;

        public DHStorageConnectionTestInternalService(
            DHStorageConfigRepository dhStorageConfigRepository,
            IRequestHeaderContext requestHeaderContext,
            PurviewMITokenClientFactory purviewMITokenClientFactory,
            FabricOnelakeClientFactory fabricOnelakeClientFactory,
            IDataEstateHealthRequestLogger logger)
        {
            this.dhStorageConfigRepository = dhStorageConfigRepository;
            this.requestHeaderContext = requestHeaderContext;
            this.logger = logger;
            this.purviewMITokenClient = purviewMITokenClientFactory.GetClient();
            this.fabricOnelakeClient = fabricOnelakeClientFactory.GetClient();
        }


        public async Task TestConnection(DHStorageConfigBaseWrapper entity, string token)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Type == DHStorageConfigWrapperDerivedTypes.Fabric)
            {
                await this.TestADLSGen2Connection((DHStorageConfigFabricWrapper)entity, token).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Unsupported storage type: {entity.Type}");
            }
        }

        private async Task TestADLSGen2Connection(DHStorageConfigFabricWrapper entity, string token)
        {
            var filename = $"purview_storage_config_connectivity_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.txt";
            var locationURL = entity.LocationURL.EndsWith("/") ? entity.LocationURL : $"{entity.LocationURL}/";
            var fileurl = new Uri(new Uri(locationURL), filename).AbsolutePath;
            try
            {
                await this.fabricOnelakeClient.CreateFile(fileurl, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Fail to create file in fabric lakehouse. FileURL: {fileurl}", ex);
                throw;
            }

            try
            {
                await this.fabricOnelakeClient.DeleteFile(fileurl, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Fail to delete file in fabric lakehouse. FileURL: {fileurl}", ex);
            }
        }
    }
}
