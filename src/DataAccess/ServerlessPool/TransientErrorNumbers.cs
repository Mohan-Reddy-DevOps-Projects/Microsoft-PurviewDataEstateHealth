// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal static class TransientErrorNumbers
{
    /// <summary>
    /// The errors in the transient error set are contained in
    /// https://azure.microsoft.com/en-us/documentation/articles/sql-database-develop-error-messages/#transient-faults-connection-loss-and-other-temporary-errors
    /// </summary>
    public static readonly HashSet<int> ErrorCodes = new()
    {
        // SQL Error Code: 2
        // Cannot connect to < server name >
        -2,

        // SQL Error Code: 4060
        // Cannot open database "%.*ls" requested by the login. The login failed.
        4060,

        // SQL Error Code: 10928
        // Resource ID: %d. The %s limit for the database is %d and has been reached.
        10928,

        // SQL Error Code: 10929
        // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
        // However, the server is currently too busy to support requests greater than %d for this database.
        10929,

        // SQL Error Code: 40197
        // You will receive this error, when the service is down due to software or hardware upgrades, hardware failures,
        // or any other failover problems. The error code (%d) embedded within the message of error 40197 provides
        // additional information about the kind of failure or failover that occurred. Some examples of the error codes are
        // embedded within the message of error 40197 are 40020, 40143, 40166, and 40540.
        40197,

        // The service is currently busy. Retry the request after 10 seconds. Incident ID: %ls. Code: %d.
        40501,

        // Database '%.*ls' on server '%.*ls' is not currently available. Please retry the connection later.
        // If the problem persists, contact customer support, and provide them the session tracing ID of '%.*ls'.
        40613,

        // Cannot open server '{0}' requested by the login. Client with IP address '{1}' is not allowed to access the server
        // Occasionally encountered this error on a certain worker node.
        40615,

        // Can not connect to the SQL pool since it is paused. Please resume the SQL pool and try again.
        42108,

        // The SQL pool is warming up. Please try again.
        42109,
    };
}
