// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.Identity;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

/// <inheritdoc/>
internal sealed class ServerlessPoolClient : IServerlessPoolClient
{
    private readonly TokenCredential tokenCredential;
    private readonly ServerlessPoolAuthConfiguration authConfig;
    private readonly ServerlessPoolConfiguration config;
    private readonly List<string> sqlEndpoints;
    private readonly IDataEstateHealthRequestLogger logger;

    public ServerlessPoolClient(
        AzureCredentialFactory credentialFactory,
        IOptions<ServerlessPoolAuthConfiguration> authConfig,
        IOptions<ServerlessPoolConfiguration> config,
        IDataEstateHealthRequestLogger logger)
    {
        this.authConfig = authConfig.Value;
        this.config = config.Value;
        this.logger = logger;
        this.sqlEndpoints = new List<string>(this.config.SqlEndpoint.Split(';'));
        this.sqlEndpoints.RemoveAll(string.IsNullOrWhiteSpace);
        this.tokenCredential = credentialFactory.CreateDefaultAzureCredential(new Uri(this.authConfig.Authority));
    }

    /// <inheritdoc/>
    public async Task Initialize()
    {
        if (this.authConfig.Enabled)
        {
            await this.GetAccessTokenAsync(CancellationToken.None);
        }
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteCommandAsync(string query, CancellationToken cancellationToken, int connectTimeout = 15, int commandTimeout = 15)
    {
        SqlConnectionStringBuilder connectionStringBuilder = this.GetConnectionString(connectTimeout);
        this.logger.LogInformation($"Sql connection with ConnectionString: {connectionStringBuilder.ConnectionString}");
        AccessToken accessToken = await this.GetAccessTokenAsync(cancellationToken);

        return await this.ExecuteNonQueryAsync(query, connectionStringBuilder, accessToken.Token, commandTimeout, cancellationToken);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IDataRecord> ExecuteQueryAsync(string query, string database, [EnumeratorCancellation] CancellationToken cancellationToken, int connectTimeout = 15, int commandTimeout = 15)
    {
        this.logger.LogInformation($"Executing the serverless sql query: {query}");
        SqlConnectionStringBuilder connectionStringBuilder = this.GetConnectionString(connectTimeout);
        // Update the connection string with database if available
        if (!string.IsNullOrEmpty(database))
        {
            connectionStringBuilder.InitialCatalog = database;
        }
        this.logger.LogInformation($"Sql connection with ConnectionString: {connectionStringBuilder.ConnectionString}");
        AccessToken accessToken = await this.GetAccessTokenAsync(cancellationToken);

        IAsyncEnumerable<IDataRecord> records = this.AccessDatabaseAsync(query, connectionStringBuilder, accessToken.Token, commandTimeout, cancellationToken);

        await foreach (IDataRecord item in records.WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }

    /// <inheritdoc/>
    public async Task<SqlConnection> GetSqlConnection(SqlCredential sqlCredential, CancellationToken cancellationToken, string database = null, bool useUserNamePassword = true, int connectTimeout = 15)
    {
        SqlConnectionStringBuilder connectionStringBuilder = this.GetConnectionString(connectTimeout, database);
        this.logger.LogInformation($"Sql connection with ConnectionString: {connectionStringBuilder.ConnectionString}");

        SqlConnection sqlConnection;
        if (!useUserNamePassword)
        {
            AccessToken accessToken = await this.GetAccessTokenAsync(cancellationToken);

            sqlConnection = new(connectionStringBuilder.ConnectionString)
            {
                AccessToken = accessToken.Token,
                StatisticsEnabled = this.config.StatisticsEnabled,
                FireInfoMessageEventOnUserErrors = false    // generate exception
            };
        }
        else
        {
            sqlConnection = new(connectionStringBuilder.ConnectionString)
            {
                Credential = sqlCredential,
                StatisticsEnabled = this.config.StatisticsEnabled,
                FireInfoMessageEventOnUserErrors = false    // generate exception
            };
        }

        void InfoMessageHandler(object sender, SqlInfoMessageEventArgs e) => this.logger.LogInformation($"Serverless: info message: " + e.Message);

        sqlConnection.InfoMessage += InfoMessageHandler;

        return sqlConnection;
    }

    private async Task<AccessToken> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        AccessToken accessToken = await this.tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { this.authConfig.Resource }), cancellationToken);

        return accessToken;
    }

    /// <summary>
    /// Invoke the SQL query and respond with an enumeration of the results.
    /// </summary>
    /// <param name="query">The SQL query.</param>
    /// <param name="connectionStringBuilder">The connection properties.</param>
    /// <param name="accessToken">The access token used to authenticate with the SQL connection.</param>
    /// <param name="commandTimeout">The time in seconds to wait for the command to execute</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async IAsyncEnumerable<IDataRecord> AccessDatabaseAsync(string query, SqlConnectionStringBuilder connectionStringBuilder, string accessToken, int commandTimeout, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using SqlConnection sqlConnection = new(connectionStringBuilder.ConnectionString)
        {
            AccessToken = accessToken,
            StatisticsEnabled = this.config.StatisticsEnabled
        };
        await using SqlCommand command = new(query, sqlConnection);
        {
            sqlConnection.InfoMessage += InfoMessageHandler;
            sqlConnection.FireInfoMessageEventOnUserErrors = false; // generate exception
            command.CommandTimeout = commandTimeout;

            await sqlConnection.OpenAsync(cancellationToken);

            await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                yield return reader;
            }

            void InfoMessageHandler(object sender, SqlInfoMessageEventArgs e) => this.logger.LogInformation($"Serverless: info message: " + e.Message);

            if (this.config.StatisticsEnabled)
            {
                IDictionary stats = sqlConnection.RetrieveStatistics();
                this.logger.LogInformation(JsonSerializer.Serialize(stats));
            }
        }
    }

    /// <summary>
    /// Invoke the SQL query and respond with the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query.</param>
    /// <param name="connectionStringBuilder">The connection properties.</param>
    /// <param name="accessToken">The access token used to authenticate with the SQL connection.</param>
    /// <param name="commandTimeout">The time in seconds to wait for the command to execute</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<int> ExecuteNonQueryAsync(string query, SqlConnectionStringBuilder connectionStringBuilder, string accessToken, int commandTimeout, CancellationToken cancellationToken)
    {
        await using SqlConnection sqlConnection = new(connectionStringBuilder.ConnectionString)
        {
            AccessToken = accessToken,
            StatisticsEnabled = this.config.StatisticsEnabled
        };
        await using SqlCommand command = new(query, sqlConnection);
        {
            sqlConnection.InfoMessage += InfoMessageHandler;
            sqlConnection.FireInfoMessageEventOnUserErrors = false; // generate exception
            command.CommandTimeout = commandTimeout;

            await sqlConnection.OpenAsync(cancellationToken);

            int count = await command.ExecuteNonQueryAsync(cancellationToken);
            this.logger.LogInformation($"Number of rows affected: {count}");
            void InfoMessageHandler(object sender, SqlInfoMessageEventArgs e) => this.logger.LogInformation($"Serverless: info message: " + e.Message);

            if (this.config.StatisticsEnabled)
            {
                System.Collections.IDictionary stats = sqlConnection.RetrieveStatistics();
                this.logger.LogInformation(JsonSerializer.Serialize(stats));
            }

            return count;
        }
    }

    /// <summary>
    /// Creates the sql connection string builder.
    /// </summary>
    /// <param name="connectTimeout">The connection timeout in seconds.</param>
    /// <param name="database"></param>
    /// <returns></returns>
    private SqlConnectionStringBuilder GetConnectionString(int connectTimeout, string database = null)
    {
        return new SqlConnectionStringBuilder
        {
            DataSource = this.GetSqlEndpoint(),
            InitialCatalog = string.IsNullOrEmpty(database) ? this.config.Database : database,
            TrustServerCertificate = false,
            Encrypt = true,
            MinPoolSize = this.config.MinPoolSize,
            MaxPoolSize = this.config.MaxPoolSize,
            Pooling = true,
            ConnectTimeout = connectTimeout,
        };
    }

    /// <summary>
    /// Retrieves the SQL endpoint.
    /// </summary>
    /// <returns></returns>
    private string GetSqlEndpoint()
    {
        return this.sqlEndpoints[0];
    }
}
