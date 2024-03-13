// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;

public interface IDataQualityOutputRepository : IGetMultipleOperation<DataQualityDataProductOutputEntity, DataQualityOutputQueryCriteria>
{
}
