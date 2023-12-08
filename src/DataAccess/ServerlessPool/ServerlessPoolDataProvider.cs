// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;

/// <summary>
/// Serverless pool data provider
/// </summary>
public interface IServerlessPoolDataProvider
{
    /// <summary>
    /// Query the dataset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="options">OData query options</param>
    /// <param name="dbSetEntity">dbSet entity to query</param>
    /// <returns></returns>
    IQueryable Query<T>(ODataQueryOptions<T> options, DbSet<T> dbSetEntity) where T : class;
}

internal class ServerlessPoolDataProvider : IServerlessPoolDataProvider
{
    private const int ODataPageSize = 10000;
    private static readonly ODataQuerySettings queryOptions = new()
    {
        PageSize = ODataPageSize,
        EnsureStableOrdering = true,
    };
    private readonly IDataEstateHealthRequestLogger logger;

    public ServerlessPoolDataProvider(IDataEstateHealthRequestLogger logger)
    {
        this.logger = logger;
    }

    public IQueryable Query<T>(ODataQueryOptions<T> options, DbSet<T> dbSetEntity) where T : class
    {
        var dimensions = new Dictionary<string, string>
            {
                { "Dataset", $"{dbSetEntity.EntityType.GetDefaultTableName()}" }
            };

        IQueryable DatasetQueryable() => options.ApplyTo(dbSetEntity.AsQueryable(), queryOptions);

        return this.ExecuteQuery(DatasetQueryable, dimensions);
    }

    private IQueryable ExecuteQuery(Func<IQueryable> func, Dictionary<string, string> dimensions)
    {
        try
        {
            return func();
        }
        catch (ODataException odex)
        {
            this.logger.LogWarning("Odata exception querying synapse.", odex);
            throw new ServiceError(
                ErrorCategory.InputError,
                ServiceErrorCode.InvalidField.Code,
                odex.Message).ToException();
        }
        catch (SqlException sqlException) when (sqlException.Number == 13807 || (sqlException.InnerException is SqlException innerSqlException && innerSqlException.Number == 13807))
        {
            // Content of directory on path '%ls' cannot be listed - case when data doesn't exist, should return 204
            this.logger.LogInformation("Error in executing synapse query; content of directory on path '%ls' cannot be listed - case when data doesn't exist", sqlException);
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError("Failed to query synapse views.", ex);
            throw new ServiceError(
                ErrorCategory.ServiceError,
                ServiceErrorCode.Unknown.Code,
                ex.Message).ToException();
        }
    }
}
