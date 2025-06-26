// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using EntityModel.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.DGP.ServiceBasics.BaseModels;

public interface IDataQualityOutputRepository : IGetMultipleOperation<DataQualityDataProductOutputEntity, DataQualityOutputQueryCriteria>
{
    Task<IBatchResults<DataQualityBusinessDomainOutputEntity>> GetMultipleForBusinessDomain(
        DataQualityOutputQueryCriteria criteria,
        CancellationToken cancellationToken,
        string continuationToken = null);
}
