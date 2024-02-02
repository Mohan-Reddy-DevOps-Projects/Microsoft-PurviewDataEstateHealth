// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient
{
    using System;

    /// <summary>
    /// The http client settings
    /// </summary>
    public class HttpClientSettings
    {
        /// <summary>
        /// The name of the http client
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The length of time the handler can be reused
        /// </summary>
        public TimeSpan? HandlerLifetime { get; set; }

        /// <summary>
        /// The length of time before the request times out
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// The user agent for the http client
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// The number of times to retry the request
        /// </summary>
        public int RetryCount { get; set; }
    }
}