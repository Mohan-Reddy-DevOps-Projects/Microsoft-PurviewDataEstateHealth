// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Extensions.Logging;
using OpenTelemetry.Audit.Geneva;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a type for logging Data Estate Health logs.
/// </summary>
public interface IDataEstateHealthRequestLogger : ILogger
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


    public void LogAudit(
        AuditOperation auditOperation,
        OperationType operationType,
        OperationResult operationResult,
        string targetResourceType,
        string targetResourceId,
        OperationCategory operationCategory = OperationCategory.ResourceManagement);
}
