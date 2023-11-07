// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal interface IServerlessQueryRequestBuilder
{
    IServerlessQueryRequest<TRecord, TEntity> Build<TRecord, TEntity>(string containerPath, Action<ClauseBuilder> buildFilter = null)
        where TRecord : class
        where TEntity : class;
}
