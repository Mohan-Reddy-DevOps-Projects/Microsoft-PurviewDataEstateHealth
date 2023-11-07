// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using static Microsoft.Azure.Purview.DataEstateHealth.DataAccess.QueryConstants;

internal interface IServerlessQueryRequestBuilder
{
    IServerlessQueryRequestBuilder CreateQuery(Type entityType, string containerPath);

    IServerlessQueryRequestBuilder AndClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal);

    IServerlessQueryRequestBuilder WhereClause(string left, string right, SQLOperator sqlOperator = SQLOperator.Equal);

    IServerlessQueryRequest<BaseRecord, BaseEntity> Build();
}
