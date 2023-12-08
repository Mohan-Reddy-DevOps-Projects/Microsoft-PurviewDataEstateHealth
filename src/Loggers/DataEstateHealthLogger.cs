// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Logger for singleton services that does not log scoped parameters
/// </summary>
public abstract class DataEstateHealthLogger
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthLogger" /> class.
    /// </summary>
    public DataEstateHealthLogger(ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger("Log");
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
        this.logger.LogTrace(exception, message);
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
        this.logger.LogInformation(exception, message);
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
        this.logger.LogWarning(exception, message);
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
        this.logger.LogError(exception, message);
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
        this.logger.LogCritical(exception, message);
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
        return null;
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
