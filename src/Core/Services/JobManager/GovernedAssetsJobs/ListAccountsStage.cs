// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.GovernedAssetsJobs;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class ListAccountStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<GovernedAssetsJobMetadata> jobCallbackUtils;

    private readonly GovernedAssetsJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IComputeGovernedAssetsSparkJobComponent billingSparkJobComponent;

    public ListAccountStage(
        IServiceScope scope,
        GovernedAssetsJobMetadata metadata,
        JobCallbackUtils<GovernedAssetsJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.billingSparkJobComponent = scope.ServiceProvider.GetService<IComputeGovernedAssetsSparkJobComponent>();
    }

    public string StageName => nameof(ListAccountStage);

    public async Task<JobExecutionResult> Execute()
    {
        // TODO
        this.metadata.GovernedAssetsJobAccounts.Add(new GovernedAssetsJobAccount()
        {
            AccountServiceModel = new AccountServiceModel(
               id: "62eba4e2-d58d-4649-866b-bb510a276032",
               tenantId: "5c168757-6e6f-4d75-8112-6c4703223677",
               defaultCatalogId: "ef0d6eaf-f9bf-4229-966e-2a554282c66c",
               processingStorageModel: new ProcessingStorageModel()
               {
                   Name = "processingwus2esraapo",
                   DnsZone = "z8",
               }),
            ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Others
        });

        this.metadata.ListAccountsStageProcessed = true;

        await Task.CompletedTask;

        return this.jobCallbackUtils.GetExecutionResult(
            JobExecutionStatus.Succeeded,
            $"{this.StageName} : postponing for next stage.",
            DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.ListAccountsStageProcessed;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
