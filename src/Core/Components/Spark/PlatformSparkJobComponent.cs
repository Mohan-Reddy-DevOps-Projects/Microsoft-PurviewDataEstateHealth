// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal sealed class PlatformSparkJobComponent : IPlatformSparkJobComponent
{
    private readonly IMetadataAccessorService metadataAccessor;
    private readonly ISparkJobManager sparkJobManager;
    private readonly IProcessingStorageManager processingStorageManager;

    public PlatformSparkJobComponent(
        IMetadataAccessorService metadataAccessor,
        ISparkJobManager sparkJobManager,
        IProcessingStorageManager processingStorageManager)
    {
        this.metadataAccessor = metadataAccessor;
        this.sparkJobManager = sparkJobManager;
        this.processingStorageManager = processingStorageManager;
    }

    /// <inheritdoc/>
    public async Task SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        Models.ProcessingStorageModel processingStorageModel = await this.processingStorageManager.Get(accountServiceModel, cancellationToken);
        Models.StorageSasRequest storageSasRequest = new()
        {
            Path = "Sink",
            Permissions = "rwdl",
            TimeToLive = TimeSpan.FromHours(1)
        };

        SparkJobRequest sparkJobRequest = new()
        {
            Configuration = new Dictionary<string, string>()
            {

            },
            ExecutorCount = 1,
            File = "test",
            Name = "test",
            RunManagerArgument = new List<string>()
            {

            },
        };

        await this.sparkJobManager.SubmitJob(accountServiceModel, sparkJobRequest, cancellationToken);
    }
}
