// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;

/// <summary>
/// Utility class with helpers for background jobs and their stages.
/// </summary>
internal class JobCallbackUtils<TMetadata> where TMetadata : StagedWorkerJobMetadata
{
    private readonly TMetadata metadata;

    internal JobCallbackUtils(TMetadata metadata)
    {
        this.metadata = metadata;
    }

    /// <summary>
    /// create job execution result exception and augment it into the metadata
    /// </summary>
    /// <param name="errorCategory"></param>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    /// <param name="status"></param>
    /// <param name="nextExecutionTime"></param>
    /// <returns></returns>
    /// <exception cref="JobExecutionResultException"></exception>
    internal JobExecutionResultException GetJobException(
        ErrorCategory errorCategory,
        ErrorCode errorCode,
        string message,
        JobExecutionStatus status,
        DateTime? nextExecutionTime = null)
    {
        this.AugmentServiceException(new ServiceError(
                errorCategory,
                errorCode.Code,
                message)
            .ToException());

        return new JobExecutionResultException(
            status,
            message,
            nextExecutionTime: nextExecutionTime);
    }

    /// <summary>
    /// Get Execution Result based on the status
    /// </summary>
    /// <param name="status"></param>
    /// <param name="message"></param>
    /// <param name="nextExecutionTime"></param>
    /// <returns></returns>
    internal JobExecutionResult GetExecutionResult(
        JobExecutionStatus status,
        string message,
        DateTime? nextExecutionTime = null)
    {
        return nextExecutionTime == null
            ? new JobExecutionResult
            {
                Status = status,
                Message = message,
                NextMetadata = this.metadata.ToJson()
            }
            : new JobExecutionResult
            {
                Status = status,
                Message = message,
                NextMetadata = this.metadata.ToJson(),
                NextExecutionTime = nextExecutionTime
            };
    }

    /// <summary>
    /// Returns a job Faulted execution result with the given job error.
    /// The job error is saved as part of the job metadata, so that it can be retrieved by any job client, including the
    /// WebRole.
    /// </summary>
    /// <param name="errorCode">The error code to log</param>
    /// <param name="message">The error message</param>
    /// <returns>The job execution result object</returns>
    internal JobExecutionResult FaultJob(ErrorCode errorCode, string message)
    {
        this.AugmentServiceException(new ServiceError(
                ErrorCategory.ServiceError,
                errorCode.Code,
                message)
            .ToException());

        return new JobExecutionResult
        {
            Status = JobExecutionStatus.Faulted,
            Message = message,
            NextMetadata = this.metadata.ToJson()
        };
    }

    /// <summary>
    /// Augment a list of service exceptions
    /// </summary>
    /// <param name="serviceException"></param>
    internal void AugmentServiceException(ServiceException serviceException)
    {
        if (serviceException == null || string.IsNullOrWhiteSpace(serviceException.Message))
        {
            return;
        }

        if (this.metadata.ServiceExceptions == null)
        {
            this.metadata.ServiceExceptions = new List<ServiceException>();
        }

        if (this.metadata.ServiceExceptions.Contains(serviceException))
        {
            return;
        }

        this.metadata.ServiceExceptions.Add(serviceException);
    }
}
