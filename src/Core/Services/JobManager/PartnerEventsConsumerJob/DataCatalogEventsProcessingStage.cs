// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using global::Azure.Messaging.EventHubs.Processor;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class DataCatalogEventsProcessingStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<PartnerEventsConsumerJobMetadata> jobCallbackUtils;

    private readonly PartnerEventsConsumerJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IPartnerEventsProcessorFactory eventProcessorFactory;

    public DataCatalogEventsProcessingStage(
        IServiceScope scope,
        PartnerEventsConsumerJobMetadata metadata,
        JobCallbackUtils<PartnerEventsConsumerJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.eventProcessorFactory = scope.ServiceProvider.GetService<IPartnerEventsProcessorFactory>();
    }

    public string StageName => nameof(DataCatalogEventsProcessingStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        IPartnerEventsProcessor processor = null;
        try
        {
            processor = this.eventProcessorFactory.Build(EventSourceType.DataCatalog);

            await processor.StartAsync();

            await processor.StopAsync();

            await processor.CommitAsync(this.metadata.ProcessingStoresCache);

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"Consumed events from {this.StageName}";
        }
        catch (Exception exception)
        {
            this.dataEstateHealthRequestLogger.LogError($"Error consuming events from {this.StageName}", exception);
            jobStageStatus = JobExecutionStatus.Failed;
            jobStatusMessage = FormattableString.Invariant($"Errored consuming events from {this.StageName}, proceeding to next source stage.");
        }
        finally
        {
            if (processor != null)
            {
                await processor.StopAsync();
            }

            // Always proceed to next stage and process events from other sources.
            this.metadata.DataCatalogEventsProcessed = true;
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.DataCatalogEventsProcessed;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
