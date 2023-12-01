// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Text;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using static Microsoft.Azure.Purview.DataEstateHealth.DataAccess.QueryConstants;

internal class ServerlessQueryRequestBuilder : IServerlessQueryRequestBuilder
{
    private readonly ServerlessPoolConfiguration serverlessPoolConfig;


    public ServerlessQueryRequestBuilder(IOptions<ServerlessPoolConfiguration> serverlessPoolConfig)
    {
        this.serverlessPoolConfig = serverlessPoolConfig.Value;
    }

    /// <inheritdoc/>
    public IServerlessQueryRequest<BaseRecord, BaseEntity> Build<TRecord>(string containerPath, Action<ClauseBuilder> buildFilter = null, string selectClause = "")
        where TRecord : BaseRecord, new()
    {
        if (string.IsNullOrEmpty(containerPath))
        {
            throw new InvalidOperationException("Container path must be set before building the query request.");
        }

      
        ClauseBuilder clauseBuilder = new();
        buildFilter?.Invoke(clauseBuilder);
        IServerlessQueryRequest<BaseRecord, BaseEntity> serverlessQueryRequest = ServerlessQueryRegistry.Instance.CreateQueryFor(typeof(TRecord));
        serverlessQueryRequest.Database = this.serverlessPoolConfig.Database;
        serverlessQueryRequest.ContainerPath = containerPath;
        serverlessQueryRequest.SelectClause = selectClause;
        serverlessQueryRequest.FilterClause = clauseBuilder.ToString();

        return serverlessQueryRequest;
    }
}

internal sealed class ClauseBuilder
{
    private readonly StringBuilder filterClause = new();

    public ClauseBuilder AndClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal)
    {
        this.AppendClause(QueryConstants.AndClause, left, right, sqlOperator);

        return this;
    }
    public ClauseBuilder WhereClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal)
    {
        this.AppendClause(QueryConstants.WhereClause, left, right, sqlOperator);

        return this;
    }

    public ClauseBuilder WhereBetweenClause(string columnName, string left, string right)
    {
        this.filterClause.Append($"{QueryConstants.WhereClause} {columnName} BETWEEN '{left}' AND '{right}' ");
        return this;
    }

    public override string ToString()
    {
        return this.filterClause.ToString();
    }

    private void AppendClause(string prefix, string left, string right, SQLOperator sqlOperator = SQLOperator.Equal)
    {
        if (right != null)
        {
            // If the filter is set to None/empty, use EQUAL operator
            if (right == string.Empty || string.Equals(right, QueryConstants.NoneFilter, StringComparison.OrdinalIgnoreCase))
            {
                right = string.Empty;
                sqlOperator = SQLOperator.Equal;
            }
            else if (string.Equals(right, QueryConstants.AppliedFilter, StringComparison.OrdinalIgnoreCase))
            {
                // If the filter is set to Applied, return non-empty values
                right = string.Empty;
                sqlOperator = SQLOperator.NotEqual;
            }
            switch (sqlOperator)
            {
                case SQLOperator.LikeWithPipe:
                    this.filterClause.Append($"{prefix}{left} LIKE '%{right}{QueryConstants.PipeDelimiter}%' ");
                    break;
                case SQLOperator.Equal:
                    this.filterClause.Append($"{prefix}{left} = '{right}' ");
                    break;
                case SQLOperator.In:
                    this.filterClause.Append($"{prefix}{left} IN ({right}) ");
                    break;
                case SQLOperator.NotEqual:
                    this.filterClause.Append($"{prefix}{left} <> '{right}' ");
                    break;
                case SQLOperator.Like:
                    this.filterClause.Append($"{prefix}{left} LIKE '%{right}%' ");
                    break;
                case SQLOperator.Greater:
                    this.filterClause.Append($"{prefix}{left} > '{right}' ");
                    break;
                case SQLOperator.Less:
                    this.filterClause.Append($"{prefix}{left} < '{right}' ");
                    break;
                case SQLOperator.GreaterOrEqual:
                    this.filterClause.Append($"{prefix}{left} >= '{right}' ");
                    break;
                case SQLOperator.LessOrEqual:
                    this.filterClause.Append($"{prefix}{left} <= '{right}' ");
                    break;
            }
        }
    }
}
