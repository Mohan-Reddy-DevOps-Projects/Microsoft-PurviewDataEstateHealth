// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Serverless query executor
/// </summary>
public class ServerlessQueryExecutor : IServerlessQueryExecutor
{
    private readonly IServerlessPoolClient client;
    private readonly IDataEstateHealthLogger logger;

    /// <summary>
    /// The amount of time to wait between retries.
    /// </summary>
    private const int TimeoutDelayInMs = 5000;

    /// <inheritdoc />
    public ServerlessQueryExecutor(
        IServerlessPoolClient client,
        IDataEstateHealthLogger logger)
    {
        this.client = client;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> ExecuteAsync<TIntermediate, TEntity>(
        IServerlessQueryRequest<TIntermediate, TEntity> request,
        CancellationToken cancellationToken)
    {
        return await this.Populate(request, cancellationToken);
    }

    private async Task<IList<TEntity>> Populate<TIntermediate, TEntity>(
        IServerlessQueryRequest<TIntermediate, TEntity> request,
        CancellationToken cancellationToken)
    {
        IList<TIntermediate> recordList = await RetryUtil.ExecuteWithRetryAsync<IList<TIntermediate>, Exception>(async retryCount =>
        {
            try
            {
                IList<TIntermediate> recordList = new List<TIntermediate>();
                await foreach (IDataRecord item in client.ExecuteQueryAsync(
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
            catch (Exception ex) when ((ex.InnerException is SqlException sqlException) && (sqlException.Number == 13807))
            {
                // Content of directory on path '%ls' cannot be listed - case when data doesn't exist, should return 204
                this.logger.LogInformation("Error in executing synapse query; content of directory on path '%ls' cannot be listed - case when data doesn't exist", ex);
                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error in executing synapse query", ex);
                throw;
            }
        }, ExceptionPredicate, maxRetries: 1, retryIntervalInMs: TimeoutDelayInMs);

        IList<TEntity> outList =(IList<TEntity>)request.Finalize(recordList);

        return outList;
    }

    private bool ExceptionPredicate(Exception arg)
    {
        return arg is SqlException se && TransientErrorNumbers.ErrorCodes.Contains(se.Number); 
    }
}
