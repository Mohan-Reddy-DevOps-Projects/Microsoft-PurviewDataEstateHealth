// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Logger;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;

/// <inheritdoc />
/// <summary>
/// Logger for singleton services that does not log scoped parameters
/// </summary>
public class DataEstateHealthLogger : IDataEstateHealthLogger
{
    private const string
        OperationName = "OperationName",
        AccountId = "AccountId",
        ResourceId = "ResourceId",
        TenantId = "TenantId",
        CorrelationId = "CorrelationId";

    private readonly IOtelInstrumentation mdmLogger;

    private readonly Dictionary<PurviewShareLogTable, ILogger> loggers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthLogger" /> class.
    /// </summary>
    /// <param name="loggerFactory">The mds logger</param>
    /// <param name="mdmLogger"></param>
    public DataEstateHealthLogger(ILoggerFactory loggerFactory, IOtelInstrumentation mdmLogger)
    {
        foreach (var logTable in Enum.GetValues<PurviewShareLogTable>())
        {
            this.loggers.Add(logTable, loggerFactory.CreateLogger(logTable.ToString()));
        }
        this.mdmLogger = mdmLogger;
    }

    /// <inheritdoc/>
    public void LogTrace(
        string message, 
        Exception exception = null, 
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        //this.Log(
        //    PurviewShareLogTable.PurviewShareLogEvent, 
        //    LogLevel.Trace, 
        //    message, 
        //    exception: exception, 
        //    isSensitive: isSensitive,
        //    operationName: operationName, 
        //    sourceFilePath: sourceFilePath, 
        //    sourceLineNumber: sourceLineNumber);
    }

    /// <inheritdoc/>
    public void LogInformation(
        string message, 
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.Log(
            PurviewShareLogTable.PurviewShareLogEvent,
            LogLevel.Information,
            message,
            exception: exception,
            isSensitive: isSensitive,
            operationName: operationName,
            sourceFilePath: sourceFilePath,
            sourceLineNumber: sourceLineNumber);
    }

    /// <inheritdoc/>
    public void LogWarning(
        string message, 
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.Log(
            PurviewShareLogTable.PurviewShareLogEvent,
            LogLevel.Warning,
            message,
            exception: exception,
            isSensitive: isSensitive,
            operationName: operationName,
            sourceFilePath: sourceFilePath,
            sourceLineNumber: sourceLineNumber);
    }

    /// <inheritdoc/>
    public void LogError(
        string message, 
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.Log(
            PurviewShareLogTable.PurviewShareLogEvent,
            LogLevel.Error,
            message,
            exception: exception,
            isSensitive: isSensitive,
            operationName: operationName,
            sourceFilePath: sourceFilePath,
            sourceLineNumber: sourceLineNumber);
    }

    /// <inheritdoc/>
    public void LogCritical(
        string message, 
        Exception exception, 
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.Log(
            PurviewShareLogTable.PurviewShareLogEvent,
            LogLevel.Critical,
            message,
            exception: exception,
            isSensitive: isSensitive,
            operationName: operationName,
            sourceFilePath: sourceFilePath,
            sourceLineNumber: sourceLineNumber);
    }

    /// <inheritdoc/>
    public void Log(
        PurviewShareLogTable logTable,
        LogLevel logLevel,
        string message,
        List<KeyValuePair<string, object>> state = null,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var requestHeaderContext = this.GetRequestHeaderContext();
        this.Log(
            logTable,
            logLevel,
            message,
            requestHeaderContext?.TenantId,
            requestHeaderContext?.AccountObjectId,
            requestHeaderContext?.CorrelationId,
            requestHeaderContext?.ResourceId,
            exception,
            state,
            isSensitive,
            operationName,
            sourceFilePath,
            sourceLineNumber
        );
    }

    /// <inheritdoc/>
    public void Log(
        PurviewShareLogTable logTable,
        LogLevel logLevel,
        string message,
        Guid? tenantId,
        Guid? accountId,
        string correlationId = null,
        string resourceId = null,
        Exception exception = null,
        List<KeyValuePair<string, object>> state = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (isSensitive)
        {
            message = $"<pii>{message}</pii>";
        }

        state ??= new List<KeyValuePair<string, object>>();

        //Need the "Error" trace level to ensure a steady stream of metrics is published.
        //If only "Critical" events are published, the Geneva metrics tooling won't work correctly.
        if (logLevel == LogLevel.Critical || logLevel == LogLevel.Error)
        {
            this.mdmLogger.LogErrorMetricValue(
                1,
                new Dictionary<string, string>
                {
                    {
                        "TraceLevel", logLevel.ToString()
                    },
                    {
                        OperationName, operationName
                    },
                    {
                        "ErrorMessage", message
                    },
                    {
                        AccountId, accountId?.ToString()
                    },
                    {
                        "RootTraceId", Activity.Current?.GetRootId()
                    }
                });

            var currentActivity = Activity.Current;

            if (currentActivity != null)
            {
                currentActivity.SetTag("env_ex_msg", message);
                currentActivity.SetTag("env_ex_stack", exception?.StackTrace);
                currentActivity.SetStatus(ActivityStatusCode.Error, message);
            }
        }

        state.Add(new KeyValuePair<string, object>(OperationName, operationName));
        state.Add(new KeyValuePair<string, object>("FileName", sourceFilePath));
        state.Add(new KeyValuePair<string, object>("LineNumber", sourceLineNumber));
        state.Add(new KeyValuePair<string, object>(AccountId, accountId ?? Guid.Empty));
        state.Add(new KeyValuePair<string, object>(TenantId, tenantId ?? Guid.Empty));
        state.Add(new KeyValuePair<string, object>(CorrelationId, correlationId ?? string.Empty));
        state.Add(new KeyValuePair<string, object>(ResourceId, resourceId ?? string.Empty));
        state.Add(new KeyValuePair<string, object>("RootTraceId", Activity.Current?.GetRootId() ?? string.Empty));

        this.loggers[logTable].Log(logLevel,
            eventId: default,
            state: state,
            exception: exception,
            formatter: (state, ex) => message ?? string.Empty
            );
    }

    /// <inheritdoc/>
    public void LogRowTelemetry(
        string snapshotId,
        string tableName,
        string rowState)
    {
        var requestHeaderContext = this.GetRequestHeaderContext();
        var state = new List<KeyValuePair<string, object>>()
        {
            new KeyValuePair<string, object>("SnapshotId", snapshotId ?? string.Empty),
            new KeyValuePair<string, object>("TableName", tableName ?? string.Empty),
            new KeyValuePair<string, object>("Message", rowState ?? string.Empty),
            new KeyValuePair<string, object>(AccountId, requestHeaderContext?.AccountObjectId),
            new KeyValuePair<string, object>(CorrelationId, requestHeaderContext?.CorrelationId)
        };

        this.loggers[PurviewShareLogTable.TelemetryLogEvent].Log(LogLevel.Information,
            eventId: default,
            state: state,
            null,
            (state, ex) => string.Empty);
    }

    /// <inheritdoc cref="IDataEstateHealthLogger" />
    public async Task<TMetadata> TraceAsync<TMetadata>(
        RetryPolicy retryPolicy,
        EndPointType endPointType,
        Func<Task<TMetadata>> actionDelegate,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string action = "")
    {
        Exception exception = null;

        return await retryPolicy.ExecuteAsync(
                async () =>
                {
                    try
                    {
                        return await actionDelegate.Invoke();
                    }
                    catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;

                        throw;
                    }
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc cref="IPurviewShareLogger" />
    public async Task TraceAsync(
        RetryPolicy retryPolicy,
        EndPointType endPointType,
        Func<Task> actionDelegate,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string action = "")
    {
        Exception exception = null;

        await retryPolicy.ExecuteAsync(
                async () =>
                {
                    try
                    {
                        await actionDelegate.Invoke();
                    }
                    catch (Exception ex)
                    {
                        exception = ex;

                        throw;
                    }
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Get the request header context for scoped logger.
    /// </summary>
    /// <returns></returns>
    protected virtual IRequestHeaderContext GetRequestHeaderContext()
    {
        return null;
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this.loggers[PurviewShareLogTable.PurviewShareLogEvent].BeginScope(state);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return this.loggers[PurviewShareLogTable.PurviewShareLogEvent].IsEnabled(logLevel);
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        this.loggers[PurviewShareLogTable.PurviewShareLogEvent].Log(logLevel, eventId, state, exception, formatter); 
    }
}
