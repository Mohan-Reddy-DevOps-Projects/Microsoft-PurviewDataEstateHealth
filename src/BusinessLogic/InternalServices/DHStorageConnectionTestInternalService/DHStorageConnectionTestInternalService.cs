// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System;
    using System.Threading.Tasks;

    public class DHStorageConnectionTestInternalService
    {
        readonly private IDataEstateHealthRequestLogger logger;
        readonly private DHStorageFabricConnectionTest fabricConnectionTest;
        readonly private DHStorageADLSGen2ConnectionTest adlsGen2ConnectionTest;

        public DHStorageConnectionTestInternalService(
            IDataEstateHealthRequestLogger logger,
            DHStorageFabricConnectionTest fabricConnectionTest,
            DHStorageADLSGen2ConnectionTest adlsGen2ConnectionTest)
        {
            this.logger = logger;
            this.fabricConnectionTest = fabricConnectionTest;
            this.adlsGen2ConnectionTest = adlsGen2ConnectionTest;
        }

        public async Task TestConnection(DHStorageConfigBaseWrapper entity, string token)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (entity.Type == DHStorageConfigWrapperDerivedTypes.Fabric)
            {
                await this.fabricConnectionTest.TestConnection((DHStorageConfigFabricWrapper)entity, token).ConfigureAwait(false);
            }
            else if (entity.Type == DHStorageConfigWrapperDerivedTypes.ADLSGen2)
            {
                await this.adlsGen2ConnectionTest.TestConnection((DHStorageConfigADLSGen2Wrapper)entity, token).ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Unsupported storage type: {entity.Type}");
            }
        }
    }
}
