// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Defines the error type from http status code for MDM logging
/// </summary>
public class ErrorType
{
    /// <summary>
    /// Client error.
    /// </summary>
    public const string Client = "Client";

    /// <summary>
    /// Server error.
    /// </summary>
    public const string Server = "Server";

    /// <summary>
    /// Server timeout.
    /// </summary>
    public const string TimeOut = "TimeOut";

    /// <summary>
    /// Get the error type for the given http status code
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public static string GetErrorType(int? statusCode)
    {
        if (statusCode != null)
        {
            if (statusCode.Value == 408)
            {
                return ErrorType.TimeOut;
            }

            if (statusCode.Value >= 400 && statusCode.Value < 500)
            {
                return ErrorType.Client;
            }

            if (statusCode.Value >= 500 && statusCode.Value < 600)
            {
                return ErrorType.Server;
            }
        }

        return string.Empty;
    }
}
