// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Data.SqlClient;
using Microsoft.Purview.DataGovernance.DataLakeAPI;

/// <summary>
/// Serverless query executor
/// </summary>
public class ServerlessQueryExecutor : IServerlessQueryExecutor
{
    private readonly IServerlessPoolClient client;
    private readonly IDataEstateHealthRequestLogger logger;

    /// <summary>
    /// The amount of time to wait between retries.
    /// </summary>
    private const int TimeoutDelayInMs = 5000;

    /// <summary>
    /// Max retry count.
    /// </summary>
    private const int MaxRetries = 2;

    /// <inheritdoc />
    public ServerlessQueryExecutor(
        IServerlessPoolClient client,
        IDataEstateHealthRequestLogger logger)
    {
        this.client = client;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> ExecuteAsync<TIntermediate, TEntity>(
        IServerlessQueryRequest<TIntermediate, TEntity> request,
        CancellationToken cancellationToken) where TEntity: BaseEntity where TIntermediate : BaseRecord
    {
        return await this.Populate(request, cancellationToken);
    }

    private async Task<IList<TEntity>> Populate<TIntermediate, TEntity>(
        IServerlessQueryRequest<TIntermediate, TEntity> request,
        CancellationToken cancellationToken) where TEntity : BaseEntity where TIntermediate : BaseRecord
    {
        this.logger.LogInformation($"Query is {request.Query}");

        IList<TIntermediate> recordList = await RetryUtil.ExecuteWithRetryAsync<IList<TIntermediate>, Exception>(async retryCount =>
        {
            try
            {
                IList<TIntermediate> recordList = new List<TIntermediate>();
                await foreach (IDataRecord item in this.client.ExecuteQueryAsync(
                                       request.Query,
                                       request.Database,
                                       cancellationToken)
                                   .WithCancellation(cancellationToken))
                {
                    TIntermediate data = request.ParseRow(item);
                    recordList.Add(data);
                }
                return recordList;
            }
            catch (SqlException sqlException) when (sqlException.Number == 13807 || (sqlException.InnerException is SqlException innerSqlException && innerSqlException.Number == 13807))
            {
                // Content of directory on path '%ls' cannot be listed - case when data doesn't exist, should return 204
                this.logger.LogInformation("Error in executing synapse query; content of directory on path '%ls' cannot be listed - case when data doesn't exist", sqlException);
                return Array.Empty<TIntermediate>();
            }
            catch (SqlException sqlException)
            {
                this.logger.LogError($"SQLException error in executing synapse query. Error number is {sqlException.Number}", sqlException);
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error in executing synapse query", ex);
                throw;
            }
        }, this.ExceptionPredicate, maxRetries: MaxRetries, retryIntervalInMs: TimeoutDelayInMs);

        IList<TEntity> outList = request.Finalize(recordList).ToList();
        return outList;
    }

    private bool ExceptionPredicate(Exception arg)
    {
        return arg is SqlException se && TransientErrorNumbers.ErrorCodes.Contains(se.Number); 
    }
}
