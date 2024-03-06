// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

public interface IDataQualityScoreRepository : IGetMultipleOperation<DataQualityScoreEntity, DataQualityScoreKey>
{
}
