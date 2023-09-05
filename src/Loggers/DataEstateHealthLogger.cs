// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Logger;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <inheritdoc />
/// <summary>
/// Logger for singleton services that does not log scoped parameters
/// </summary>
public class DataEstateHealthLogger : IDataEstateHealthLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataEstateHealthLogger" /> class.
    /// </summary>
    public DataEstateHealthLogger()
    {
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void LogRowTelemetry(
        string snapshotId,
        string tableName,
        string rowState)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        throw new NotImplementedException();
    }
}
