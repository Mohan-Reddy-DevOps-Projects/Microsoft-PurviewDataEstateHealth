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
    private IServerlessQueryRequest<BaseRecord, BaseEntity> serverlessQueryRequest;
    private StringBuilder filterClause;

    public ServerlessQueryRequestBuilder(IOptions<ServerlessPoolConfiguration> serverlessPoolConfig)
    { 
        this.serverlessPoolConfig = serverlessPoolConfig.Value;
        this.filterClause = new StringBuilder();
    }

    public IServerlessQueryRequest<BaseRecord, BaseEntity> Build()
    {
        this.serverlessQueryRequest.FilterClause += filterClause.ToString();
        return this.serverlessQueryRequest;
    }

    public IServerlessQueryRequestBuilder CreateQuery(Type entityType, string containerPath)
    {
        this.serverlessQueryRequest = ServerlessQueryRegistry.Instance.CreateQueryFor(entityType);
        serverlessQueryRequest.Database = this.serverlessPoolConfig.Database;
        serverlessQueryRequest.ContainerPath = containerPath;
        return this;
    }

    public IServerlessQueryRequestBuilder AndClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal)
    {
        this.AppendClause(QueryConstants.AndClause, left, right, sqlOperator);
        return this;
    }

    public IServerlessQueryRequestBuilder WhereClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal)
    {
        this.AppendClause(QueryConstants.WhereClause, left, right, sqlOperator);
        return this;
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
                    filterClause.Append($"{prefix}{left} LIKE '%{right}{QueryConstants.PipeDelimiter}%' ");
                    break;
                case SQLOperator.Equal:
                    filterClause.Append($"{prefix}{left} = '{right}' ");
                    break;
                case SQLOperator.In:
                    filterClause.Append($"{prefix}{left} IN ({right}) ");
                    break;
                case SQLOperator.NotEqual:
                    filterClause.Append($"{prefix}{left} <> '{right}' ");
                    break;
                case SQLOperator.Like:
                    filterClause.Append($"{prefix}{left} LIKE '%{right}%' ");
                    break;
                case SQLOperator.Greater:
                    filterClause.Append($"{prefix}{left} > '{right}' ");
                    break;
                case SQLOperator.Less:
                    filterClause.Append($"{prefix}{left} < '{right}' ");
                    break;
                case SQLOperator.GreaterOrEqual:
                    filterClause.Append($"{prefix}{left} >= '{right}' ");
                    break;
                case SQLOperator.LessOrEqual:
                    filterClause.Append($"{prefix}{left} <= '{right}' ");
                    break;
            }
        }
    }
}
