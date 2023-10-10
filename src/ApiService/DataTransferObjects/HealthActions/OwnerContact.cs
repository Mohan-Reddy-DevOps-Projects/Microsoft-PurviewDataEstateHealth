// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.ComponentModel;

/// <summary>
/// Owner contact class.
/// </summary>
public partial class OwnerContact
{
    /// <summary>
    /// Owner display name
    /// </summary>
    [ReadOnly(true)]
    public string DisplayName { get; internal set; }

    /// <summary>
    /// Owner Id
    /// </summary>
    [ReadOnly(true)]
    public Guid ObjectId { get; internal set; }
}
