// <copyright file="TimedLogger.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers
{
    using System;
    using System.Diagnostics;

    public class TimedLogger : IDisposable
    {
        private readonly IDataEstateHealthRequestLogger logger;
        private readonly Stopwatch timer;
        private string message;

        public long ElapsedTime => this.timer?.ElapsedMilliseconds ?? 0;

        public TimedLogger(IDataEstateHealthRequestLogger logger, string message)
        {
            this.message = message;
            this.logger = logger;
            this.timer = Stopwatch.StartNew();
            this.logger.LogInformation($"Start: {this.message}");
        }

        public void Dispose()
        {
            this.timer.Stop();
            this.logger.LogInformation($"Complete: {this.message}, TotalMilliseconds: {this.ElapsedTime}");
            GC.SuppressFinalize(this);
        }
    }

    public static class TimedLoggerExtensions
    {
        public static TimedLogger LogElapsed(this IDataEstateHealthRequestLogger logger, string message)
        {
            return new TimedLogger(logger, message);
        }
    }
}
