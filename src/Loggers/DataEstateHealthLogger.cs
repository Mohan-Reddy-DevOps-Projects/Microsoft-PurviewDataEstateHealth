// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc />
/// <summary>
/// Logger for singleton services that does not log scoped parameters
/// </summary>
public class DataEstateHealthLogger : IDataEstateHealthLogger, IDataEstateHealthRequestLogger
{
    private readonly IOtelInstrumentation mdmLogger;

    private readonly Dictionary<DataEstateHealthLogTable, ILogger> loggers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthLogger" /> class.
    /// </summary>
    /// <param name="loggerFactory">The mds logger</param>
    /// <param name="mdmLogger"></param>
    public DataEstateHealthLogger(ILoggerFactory loggerFactory, IOtelInstrumentation mdmLogger)
    {
        foreach (var logTable in Enum.GetValues<DataEstateHealthLogTable>())
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
    }

    /// <inheritdoc/>
    public void Log(
        DataEstateHealthLogTable logTable,
        LogLevel logLevel,
        string message,
        List<KeyValuePair<string, object>> state = null,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
    }

    /// <inheritdoc/>
    public void Log(
        DataEstateHealthLogTable logTable,
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
    }

    /// <inheritdoc/>
    public void LogRowTelemetry(
        string snapshotId,
        string tableName,
        string rowState)
    {
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
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
    }
}
