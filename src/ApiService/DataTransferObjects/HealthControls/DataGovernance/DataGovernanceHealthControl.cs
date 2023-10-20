// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A DataGovernance health control class.
/// </summary>
public class DataGovernanceHealthControl : HealthControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGovernanceHealthControl"/> class.
    /// </summary>
    public DataGovernanceHealthControl()
    {
        this.Kind = HealthControlKind.DataGovernance;
        this.Properties = new DataGovernanceHealthControlProperties();
    }

    /// <summary>
    /// DataGovernance Health Control properties.
    /// </summary>
    [Required]
    public DataGovernanceHealthControlProperties Properties { get; set; }
}
