// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

internal interface IServerlessQueryRequestBuilder
{
    IServerlessQueryRequest<BaseRecord, BaseEntity> Build<TRecord>(string containerPath, Action<ClauseBuilder> buildFilter = null, string selectClause = "")
        where TRecord : BaseRecord, new();

    IServerlessQueryRequest<BaseRecord, BaseEntity> BuildExternalTableQuery<TRecord>(Action<ClauseBuilder> buildFilter = null, string selectClause = "")
        where TRecord : BaseRecord, new();
}
