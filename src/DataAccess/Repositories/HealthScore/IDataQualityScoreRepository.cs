// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Data quality repository interface
/// </summary>
public interface IDataQualityScoreRepository :
    IGetSingleOperation<DataQualityScoresModel, DomainDataQualityScoreKey>,
    IGetSingleOperation<DataQualityScoresModel, DataProductDataQualityScoreKey>,
    IGetSingleOperation<DataQualityScoresModel, DataAssetDataQualityScoreKey>,
    IGetMultipleOperation<DataQualityScoreModel, DomainDataQualityScoreKey>,
    IGetMultipleOperation<DataQualityScoreModel, DataProductDataQualityScoreKey>,
    IGetMultipleOperation<DataQualityScoreModel, DataAssetDataQualityScoreKey>,
    ILocationBased<IDataQualityScoreRepository>
{
}
