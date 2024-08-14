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
                id: "4285effc-d0a0-43bb-88b9-34932df50117",
                tenantId: "12d98746-0b5a-4778-8bd0-449994469062",
                processingStorageModel: new ProcessingStorageModel()
                {
                    Name = "",
                    DnsZone = "",
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
