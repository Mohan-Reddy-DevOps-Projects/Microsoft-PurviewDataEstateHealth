// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Logger for singleton services that does not log scoped parameters
/// </summary>
public abstract class DataEstateHealthLogger
{
    private readonly ILogger logger;

    private readonly EnvironmentConfiguration environmentConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthLogger" /> class.
    /// </summary>
    public DataEstateHealthLogger(ILoggerFactory loggerFactory, IOptions<EnvironmentConfiguration> environmentConfiguration)
    {
        this.logger = loggerFactory.CreateLogger("Log");
        this.environmentConfiguration = environmentConfiguration.Value;
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
        var state = this.GetAdditionalColumns(operationName, sourceFilePath, sourceLineNumber);

        this.Log(
            LogLevel.Trace,
            default,
            state,
            null,
            (state, ex) => message);
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
        var state = this.GetAdditionalColumns(operationName, sourceFilePath, sourceLineNumber);

        this.Log(
            LogLevel.Information,
            default,
            state,
            null,
            (state, ex) => message);
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
        var state = this.GetAdditionalColumns(operationName, sourceFilePath, sourceLineNumber);

        this.Log(
            LogLevel.Warning,
            default,
            state,
            exception,
            (state, ex) => message);
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
        var state = this.GetAdditionalColumns(operationName, sourceFilePath, sourceLineNumber);

        this.Log(
            LogLevel.Error,
            default,
            state,
            exception,
            (state, ex) => message);
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
        var state = this.GetAdditionalColumns(operationName, sourceFilePath, sourceLineNumber);

        this.Log(
            LogLevel.Critical,
            default,
            state,
            exception,
            (state, ex) => message);
    }

    /// <inheritdoc/>
    private void Log(
        LogLevel logLevel,
        EventId eventId,
        List<KeyValuePair<string, object>> state,
        Exception exception,
        Func<List<KeyValuePair<string, object>>, Exception, string> formatter)
    {
        if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical || logLevel == LogLevel.Warning)
        {
            var currentActivity = Activity.Current;

            if (currentActivity != null)
            {
                currentActivity.SetTag("env_ex_msg", exception?.Message ?? string.Empty);
                currentActivity.SetTag("env_ex_stack", exception?.StackTrace ?? string.Empty);
            }
        }

        this.logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc/>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        this.logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc/>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this.logger.BeginScope(state);
    }

    private List<KeyValuePair<string, object>> GetAdditionalColumns(string operationName, string sourceFilePath, int sourceLineNumber)
    {
        var requestHeaderContext = this.GetRequestHeaderContext();

        return new List<KeyValuePair<string, object>>
        {
            new KeyValuePair<string, object>("OperationName", operationName),
            new KeyValuePair<string, object>("FileName", sourceFilePath),
            new KeyValuePair<string, object>("LineNumber", sourceLineNumber),
            new KeyValuePair<string, object>("AccountId", requestHeaderContext?.AccountObjectId ?? Guid.Empty),
            new KeyValuePair<string, object>("TenantId", requestHeaderContext?.TenantId ?? Guid.Empty),
            new KeyValuePair<string, object>("CorrelationId", requestHeaderContext?.CorrelationId ?? string.Empty),
            new KeyValuePair<string, object>("RootTraceId", Activity.Current?.GetRootId() ?? string.Empty)
        };
    }

    /// <summary>
    /// Get the request header context for scoped logger.
    /// </summary>
    /// <returns></returns>
    protected virtual IRequestHeaderContext GetRequestHeaderContext()
    {
        return null;
    }
}
