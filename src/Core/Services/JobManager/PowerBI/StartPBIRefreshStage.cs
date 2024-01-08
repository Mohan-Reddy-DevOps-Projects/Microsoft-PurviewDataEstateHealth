// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class StartPBIRefreshStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils;
    private readonly IServiceScope scope;
    private readonly StartPBIRefreshMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IRefreshComponent refreshComponent;

    public StartPBIRefreshStage(
        IServiceScope scope,
        StartPBIRefreshMetadata metadata,
        JobCallbackUtils<StartPBIRefreshMetadata> jobCallbackUtils)
    {
        this.scope = scope;
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.refreshComponent = scope.ServiceProvider.GetService<IRefreshComponent>();
    }

    public string StageName => nameof(StartPBIRefreshStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        try
        {
            IList<RefreshLookup> refreshLookups = await this.refreshComponent.RefreshDatasets(Guid.Parse(this.metadata.Account.Id), CancellationToken.None);

            this.metadata.RefreshLookups = refreshLookups;

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"Completed stage {this.StageName}";
        }
        catch (Exception exception)
        {
            this.logger.LogError($"Error starting PBI refresh from {this.StageName}", exception);
            jobStageStatus = JobExecutionStatus.Failed;
            jobStatusMessage = FormattableString.Invariant($"Errored starting PBI refresh from {this.StageName}, proceeding to next stage.");
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.RefreshLookups.Count != 0;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
