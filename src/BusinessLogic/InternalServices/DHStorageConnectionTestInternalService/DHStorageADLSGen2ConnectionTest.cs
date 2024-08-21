// <copyright file="DHStorageADLSGen2ConnectionTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using global::Azure.Storage.Files.DataLake;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class DHStorageADLSGen2ConnectionTest : IDHStorageConnectionTest<DHStorageConfigADLSGen2Wrapper>
    {
        readonly private IDataEstateHealthRequestLogger logger;

        public DHStorageADLSGen2ConnectionTest(
            IDataEstateHealthRequestLogger logger)
        {
            this.logger = logger;
        }

        public async Task TestConnection(DHStorageConfigADLSGen2Wrapper entity, string token)
        {
            var endpoint = entity.Endpoint;
            var fileSystemName = $"purviewbyoctestconnection{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            var fileName = "file.txt";
            try
            {
                var dataLakeServiceClient = new DataLakeServiceClient(new Uri(endpoint), new DHStorageConnectionMIToken(token));
                var fileSystemClient = await dataLakeServiceClient.CreateFileSystemAsync(fileSystemName).ConfigureAwait(false);
                var directoryClient = fileSystemClient.Value.GetFileClient(fileName);
                await directoryClient.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"))).ConfigureAwait(false);
                await dataLakeServiceClient.DeleteFileSystemAsync(fileSystemName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Fail to create file in ADLS Gen2. Endpoint: {endpoint}, FileSystemName: {fileSystemName}, FileName: {fileName}", ex);
                throw;
            }
        }
    }

}
