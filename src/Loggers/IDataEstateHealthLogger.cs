// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a type for logging Data Estate Health logs.
/// </summary>
public interface IDataEstateHealthLogger : ILogger
{
    /// <summary>
    /// Logs a trace event to the DataEstateHealthLogEvent table.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="isSensitive"></param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath"> The path of the source file calling the log method (automatically injected at compile time)</param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void LogTrace(
        string message,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Logs an information event to the DataEstateHealthLogEvent table.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="isSensitive"></param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath"> The path of the source file calling the log method (automatically injected at compile time)</param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void LogInformation(
        string message,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Logs a warning event to the DataEstateHealthLogEvent table.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="isSensitive"></param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath"> The path of the source file calling the log method (automatically injected at compile time)</param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void LogWarning(
        string message,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Logs an error event to the DataEstateHealthLogEvent table.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="isSensitive"></param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath"> The path of the source file calling the log method (automatically injected at compile time)</param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void LogError(
        string message,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Logs a critical event to the DataEstateHealthLogEvent table.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="isSensitive"></param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath"> The path of the source file calling the log method (automatically injected at compile time)</param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void LogCritical(
        string message,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Log an event to Geneva
    /// </summary>
    /// <param name="logTable">The table to log the event to</param>
    /// <param name="logLevel">Level of log</param>
    /// <param name="message">Log message</param>
    /// <param name="state">Fields to add to the log entry</param>
    /// <param name="exception">optional exception message</param>
    /// <param name="isSensitive">PII flag - will strip out PII if marked "true" </param>
    /// <param name="correlationId">Correlation ID - used when requestHeaderContext isn't available</param>
    /// <param name="tenantId">Tenant ID - used when requestHeaderContext isn't available</param>
    /// <param name="accountId">Account ID - used when requestHeaderContext isn't available</param>
    /// <param name="resourceId">Resource ID to log</param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath">
    /// The path of the source file calling the log method (automatically injected at compile
    /// time)
    /// </param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void Log(
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
        [CallerLineNumber] int sourceLineNumber = 0);

    /// <summary>
    /// Log an event to Geneva
    /// </summary>
    /// <param name="logTable">The table to log the event to</param>
    /// <param name="logLevel">Level of log</param>
    /// <param name="message">Log message</param>
    /// <param name="state">Fields to add to the log entry</param>
    /// <param name="exception">optional exception message</param>
    /// <param name="isSensitive">PII flag - will strip out PII if marked "true" </param>    
    /// <param name="operationName">Calling function name (automatically injected at compile time)</param>
    /// <param name="sourceFilePath">
    /// The path of the source file calling the log method (automatically injected at compile
    /// time)
    /// </param>
    /// <param name="sourceLineNumber">The source line number (automatically injected at compile time)</param>
    void Log(
        DataEstateHealthLogTable logTable,
        LogLevel logLevel,
        string message,
        List<KeyValuePair<string, object>> state = null,
        Exception exception = null,
        bool isSensitive = false,
        [CallerMemberName] string operationName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0);
}
