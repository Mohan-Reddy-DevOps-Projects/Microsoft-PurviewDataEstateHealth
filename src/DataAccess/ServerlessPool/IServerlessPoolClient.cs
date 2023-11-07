// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Interface for Synapse Serverless Pool service
/// </summary>
public interface IServerlessPoolClient
{
    /// <summary>
    /// Initialize the serverless pool client
    /// </summary>
    /// <returns></returns>
    Task Initialize();

    /// <summary>
    /// Invoke the SQL query and respond with an enumeration of the results.
    /// </summary>
    /// <param name="query">The SQL query.</param>
    /// <param name="database">The database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="connectTimeout">The time in seconds to wait for a connection to the server before terminating.</param>
    /// <param name="commandTimeout">The time in seconds to wait for the command to execute.</param>
    /// <returns></returns>
    IAsyncEnumerable<IDataRecord> ExecuteQueryAsync(string query, string database, CancellationToken cancellationToken, int connectTimeout = 15, int commandTimeout = 15);

    /// <summary>
    /// Invoke the SQL query and respond with the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="connectTimeout">The time in seconds to wait for a connection to the server before terminating.</param>
    /// <param name="commandTimeout">The time in seconds to wait for the command to execute.</param>
    /// <returns></returns>
    Task<int> ExecuteCommandAsync(string query, CancellationToken cancellationToken, int connectTimeout = 15, int commandTimeout = 15);

    /// <summary>
    /// Create a SQL connection using the either integrated authentication token from the <see cref="ServerlessPoolAuthConfiguration"/>, 
    /// OR login and passowrd created for the account's schema
    /// </summary>
    /// <param name="sqlCredential">The SQL credentials used for authenticating database connections.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="database">The database.</param>
    /// <param name="useUserNamePassword">Whether or not to execute SQL operations with a username and password.</param>
    /// <param name="connectTimeout">The time in seconds to wait for a connection to the server before terminating.</param>
    /// <returns>SQL Connection</returns>
    Task<SqlConnection> GetSqlConnection(SqlCredential sqlCredential, CancellationToken cancellationToken, string database = null, bool useUserNamePassword = true, int connectTimeout = 15);
}
