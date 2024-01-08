// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Data Quality 
/// </summary>
public interface IDataQualityScoreComponent :
    IRetrieveEntityByIdOperation<Guid, IHealthScoreModel<HealthScoreProperties>>,
    IComponent<IDataQualityScoreContext>
{
}
