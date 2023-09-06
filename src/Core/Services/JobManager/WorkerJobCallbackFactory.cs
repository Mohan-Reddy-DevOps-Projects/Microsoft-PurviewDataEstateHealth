// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;
using Newtonsoft.Json;
using JobLogger = WindowsAzure.ResourceStack.Common.BackgroundJobs.JobLogger;

/// <summary>
/// Callback factory fo creating jobs
/// TODO 1151182 Inject request header context when we have the worker service and put back GetRequestHeaderContext()
/// </summary>
internal class WorkerJobCallbackFactory : JobCallbackFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IDataEstateHealthLogger dataEstateHealthLogger;

    /// <summary>
    /// Constructor for the <see cref="WorkerJobCallbackFactory"/>.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public WorkerJobCallbackFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.dataEstateHealthLogger = this.serviceProvider.GetService<IDataEstateHealthLogger>();
    }

    public override JobDelegate CreateInstance(JobLogger jobLogger, Type callbackType, BackgroundJob job)
    {
        if (this.IsWorkerJobCallback(callbackType))
        {
            IServiceScope scope = this.serviceProvider.CreateScope();
            IRequestHeaderContextFactory requestContextFactory =
                scope.ServiceProvider.GetRequiredService<IRequestHeaderContextFactory>();

            JobDelegate jobDelegate = null;

            try
            {
                RequestHeaderContext requestHeaderContext = this.GetRequestHeaderContext(job.Metadata);
                requestContextFactory.SetContext(requestHeaderContext);
                RequestCorrelationContext.Current.Initialize(requestHeaderContext.RequestCorrelationContext);

                jobDelegate = (JobDelegate)Activator.CreateInstance(callbackType, scope);
            }
            catch (SerializationException exception)
            {
                this.dataEstateHealthLogger.LogCritical(
                    "Unable to parse WorkerJobMetadata - " + job.Metadata?.ToJson(),
                    exception);
                jobDelegate = new FaultedJobDelegate(job);
            }

            return jobDelegate;
        }

        return base.CreateInstance(jobLogger, callbackType, job);
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

    private RequestHeaderContext GetRequestHeaderContext(string metadata)
    {
        if (!string.IsNullOrWhiteSpace(metadata))
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            StagedWorkerJobMetadata workerJobMetadata =
                JsonConvert.DeserializeObject<StagedWorkerJobMetadata>(metadata, jsonSerializerSettings);

            if (workerJobMetadata?.RequestHeaderContext == null)
            {
                this.dataEstateHealthLogger.LogCritical(
                    "Unable to get requestHeaderContext from metadata - " + metadata.ToJson());
            }
            else
            {
                return workerJobMetadata.RequestHeaderContext;
            }
        }

        return new RequestHeaderContext(new HttpContextAccessor());
    }
}
