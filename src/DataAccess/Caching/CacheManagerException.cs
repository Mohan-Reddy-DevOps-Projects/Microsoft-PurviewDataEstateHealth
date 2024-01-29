// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Custom exception thrown by the cache manager
/// </summary>
internal sealed class CacheManagerException : Exception
{
    /// <summary>
    /// Redis cache is disabled if this value is not Zero - can be used as RetryAfter response header
    /// </summary>
    public TimeSpan DisableOnErrorInterval { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CacheManagerException()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Error message</param>
    public CacheManagerException(string message) : base(message)
    {
    }

    /// <summary>
    /// Constructor with inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    public CacheManagerException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
