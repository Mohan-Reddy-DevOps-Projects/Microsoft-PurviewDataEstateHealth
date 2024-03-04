// <copyright file="TipLogger.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
#nullable enable
    public static class TipLoggerExtensions
    {
        public static void LogTipInformation(this IDataEstateHealthRequestLogger logger, string message, JObject? additionalInfo = null)
        {
            var tipLogInfo = new JObject
            {
                { "type" , "dgh-tipLog" },
                { "logInfo", message },
                { "additionalInfo", additionalInfo ?? new JObject()}
            };
            logger.LogInformation($"Start: {JsonConvert.SerializeObject(tipLogInfo)}");
        }
    }
#nullable restore
}
