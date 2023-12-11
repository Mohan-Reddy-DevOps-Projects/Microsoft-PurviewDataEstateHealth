// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Components;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing data quality score collections.
/// </summary>
public interface IDataQualityScoreCollectionComponent :
    IRetrieveEntityCollectionOperations<IHealthScoreModel<HealthScoreProperties>>, INavigable<Guid, IDataQualityScoreCollectionComponent>,
    IComponent<IDataQualityScoreContext>
{
}
