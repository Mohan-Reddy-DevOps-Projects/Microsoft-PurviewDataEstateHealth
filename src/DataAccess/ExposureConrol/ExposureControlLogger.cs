// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.ExposureControlLibrary;
using System.Diagnostics;

internal sealed class ExposureControlLogger : IExposureControlLogger
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly TraceEventType logLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExposureControlLogger"/> class.
    /// </summary>
    /// <param name="logger">Logger supplied by the service, exposed to the EC library through its own interface.</param>
    /// <param name="loggingLevel">Logging level for this logger.</param>
    public ExposureControlLogger(IDataEstateHealthRequestLogger logger, string loggingLevel)
    {
        this.logger = logger;
        try
        {
            this.logLevel = Enum.Parse<TraceEventType>(loggingLevel);
        }
        catch (ArgumentException)
        {
            this.logLevel = TraceEventType.Warning;
        }
    }

    /// <inheritdoc/>
    public void Log(TraceEventType traceEventType, string message, params object[] args)
    {
        if (traceEventType <= this.logLevel)
        {
            string logMessage = string.Format(default, message, args);
            switch (traceEventType)
            {
                case TraceEventType.Verbose:
                    this.logger.LogTrace(logMessage);
                    break;
                case TraceEventType.Information:
                    this.logger.LogInformation(logMessage);
                    break;
                case TraceEventType.Warning:
                    this.logger.LogWarning(logMessage);
                    break;
                case TraceEventType.Error:
                    this.logger.LogError(logMessage, null);
                    break;
                case TraceEventType.Critical:
                    this.logger.LogCritical(logMessage, null);
                    break;
            }
        }
    }
}
