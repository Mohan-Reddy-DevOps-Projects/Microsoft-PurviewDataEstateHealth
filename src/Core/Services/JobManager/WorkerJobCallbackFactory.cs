// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Runtime.Serialization;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;
using JobLogger = WindowsAzure.ResourceStack.Common.BackgroundJobs.JobLogger;

/// <summary>
/// Callback factory for creating jobs
/// </summary>
internal class WorkerJobCallbackFactory : JobCallbackFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IDataEstateHealthRequestLogger logger;
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    /// <summary>
    /// Constructor for the <see cref="WorkerJobCallbackFactory"/>.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public WorkerJobCallbackFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.logger = this.serviceProvider.GetService<IDataEstateHealthRequestLogger>();
    }

    public override JobDelegate CreateInstance(JobLogger jobLogger, Type callbackType, BackgroundJob job)
    {
        if (this.IsWorkerJobCallback(callbackType))
        {
            IServiceScope scope = this.serviceProvider.CreateScope();

            JobDelegate jobDelegate = null;

            try
            {
                this.UpdateDerivedMetadataProperties(job);
                jobDelegate = (JobDelegate)Activator.CreateInstance(callbackType, scope);
            }
            catch (SerializationException exception)
            {
                this.logger.LogCritical(
                    "Unable to parse WorkerJobMetadata - " + job.Metadata?.ToJson(),
                    exception);
                jobDelegate = new FaultedJobDelegate(job);
            }

            return jobDelegate;
        }

        return base.CreateInstance(jobLogger, callbackType, job);
    }

    private void UpdateDerivedMetadataProperties(BackgroundJob job)
    {
        CallbackRequestContext requestHeaderContext = this.GetRequestHeaderContext(job.Metadata);
        RequestCorrelationContext context = new()
        {
            CorrelationId = requestHeaderContext.CorrelationId
        };
        RequestCorrelationContext.Current.Initialize(context);
    }

    private bool IsWorkerJobCallback(Type callbackType)
    {
        if (callbackType.IsGenericType)
        {
            Type genericDef = callbackType.GetGenericTypeDefinition();
            if (genericDef == typeof(StagedWorkerJobCallback<>))
            {
                return true;
            }
        }

        return callbackType.BaseType != null && this.IsWorkerJobCallback(callbackType.BaseType);
    }

    private CallbackRequestContext GetRequestHeaderContext(string metadata)
    {
        StagedWorkerJobMetadata workerJobMetadata = JsonConvert.DeserializeObject<StagedWorkerJobMetadata>(metadata, jsonSerializerSettings);
        if (workerJobMetadata?.RequestContext == null)
        {
            this.logger.LogCritical("Unable to get requestHeaderContext from metadata - " + metadata.ToJson());
        }

        return workerJobMetadata.RequestContext;
    }
}
