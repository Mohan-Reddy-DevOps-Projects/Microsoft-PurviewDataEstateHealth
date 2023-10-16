// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

/// <summary>
/// Health control state enum.
/// </summary>
public enum HealthControlState
{
    /// <summary>
    /// Active
    /// </summary>
    Active = 1,

    /// <summary>
    /// Resolved
    /// </summary>
    Resolved
}
