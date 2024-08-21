// <copyright file="DHStorageFabricConnectionTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System;
    using System.Threading.Tasks;

    public class DHStorageFabricConnectionTest : IDHStorageConnectionTest<DHStorageConfigFabricWrapper>
    {
        readonly private IDataEstateHealthRequestLogger logger;
        readonly private FabricOnelakeClient fabricOnelakeClient;

        public DHStorageFabricConnectionTest(
            FabricOnelakeClientFactory fabricOnelakeClientFactory,
            IDataEstateHealthRequestLogger logger)
        {
            this.logger = logger;
            this.fabricOnelakeClient = fabricOnelakeClientFactory.GetClient();
        }

        public async Task TestConnection(DHStorageConfigFabricWrapper entity, string token)
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
