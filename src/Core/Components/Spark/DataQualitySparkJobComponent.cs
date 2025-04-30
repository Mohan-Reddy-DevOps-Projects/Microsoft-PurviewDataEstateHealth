// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.SynapseSqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DataQualitySparkJobComponent : IDataQualitySparkJobComponent
{
    private readonly ISparkJobManager sparkJobManager;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;

    public DataQualitySparkJobComponent(
        ISparkJobManager sparkJobManager,
        IProcessingStorageManager processingStorageManager,
        IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration)
    {
        this.sparkJobManager = sparkJobManager;
        this.processingStorageManager = processingStorageManager;
        this.serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel processingStorageModel = await this.processingStorageManager.Get(accountServiceModel, cancellationToken);
        string containerName = accountServiceModel.DefaultCatalogId;
        Uri sinkSasUri = await this.GetSinkSasUri(processingStorageModel, containerName, cancellationToken);
        SparkJobRequest sparkJobRequest = this.GetSparkJobRequest(sinkSasUri, containerName, sinkSasUri.Host);
        return await this.sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken);
    }

    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(accountServiceModel, batchId, cancellationToken);

    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(jobInfo, cancellationToken);

    private async Task<Uri> GetSinkSasUri(Models.ProcessingStorageModel processingStorageModel, string containerName, CancellationToken cancellationToken)
    {
        Models.StorageSasRequest storageSasRequest = new()
        {
            Path = "/",
            Permissions = "rwdlac",
            TimeToLive = TimeSpan.FromHours(1)
        };

        return await this.processingStorageManager.GetProcessingStorageSasUri(processingStorageModel, storageSasRequest, containerName, cancellationToken);
    }

    private SparkJobRequest GetSparkJobRequest(Uri sasUri, string containerName, string sinkLocation)
    {
        return new()
        {
            Configuration = this.GetSinkConfiguration(sasUri, containerName),
            ExecutorCount = 1,
            File = $"abfss://dghsynapse@{this.serverlessPoolConfiguration.StorageAccount}.dfs.core.windows.net/dataquality_spark_job_main.py",
            Name = "DataQualitySparkJob",
            RunManagerArgument =
            [
                $"abfss://dghsynapse@{this.serverlessPoolConfiguration.StorageAccount}.dfs.core.windows.net/DataEstateHealthLibrary.zip",
                $"abfss://{containerName}@{sinkLocation}/Source/",
                $"abfss://{containerName}@{sinkLocation}/Sink/",
                "DataQuality/",
                "_Deleted/"
            ],
        };
    }

    private Dictionary<string, string> GetSinkConfiguration(Uri sasUri, string containerName)
    {
        return new Dictionary<string, string>()
        {
            {$"fs.azure.account.auth.type.{sasUri.Host}", "SAS"},
            {$"fs.azure.sas.token.provider.type.{sasUri.Host}", "com.microsoft.azure.synapse.tokenlibrary.ConfBasedSASProvider" },
            {$"spark.storage.synapse.{containerName}.{sasUri.Host}.sas", sasUri.Query[1..] }
        };
    }
}
