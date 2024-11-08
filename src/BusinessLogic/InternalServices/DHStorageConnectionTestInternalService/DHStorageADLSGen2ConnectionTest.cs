// <copyright file="DHStorageADLSGen2ConnectionTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using global::Azure.Storage.Files.DataLake;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
    using System;
    using System.IO;
    using System.Security.Policy;
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
            var endpoint = string.Empty;
            try
            {
                // Parse the URI and extract components
                endpoint = entity.Endpoint;
                var uri = new Uri(endpoint);
                var storageAccountName = uri.Host.Split('.')[0];
                var fileSystemName = uri.AbsolutePath.TrimStart('/').Split('/')[0];

                // Set up the DataLakeServiceClient
                var dataLakeServiceClient = new DataLakeServiceClient(new Uri($"https://{uri.Host}"),
                    new DHStorageConnectionMIToken(token));

                // Upload the file content and then delete it to check write access
                var fileClient = dataLakeServiceClient.GetFileSystemClient(fileSystemName)
                    .GetFileClient("temp____deh_____byoc_____test_____file.txt");

                var content = Encoding.UTF8.GetBytes("Hello, World!");
                using (var stream = new MemoryStream(content))
                {
                    await fileClient.UploadAsync(stream).ConfigureAwait(false);
                    await fileClient.DeleteIfExistsAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Fail to create file in ADLS Gen2. Endpoint: {endpoint}", ex);
                throw;
            }
        }
    }

}
