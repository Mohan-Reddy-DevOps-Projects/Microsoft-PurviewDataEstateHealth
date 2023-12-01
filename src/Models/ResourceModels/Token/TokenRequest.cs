// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Token model.
/// </summary>
public class TokenModel
{
    /// <summary>
    /// Gets or sets the dataset ids.
    /// </summary>
    public HashSet<Guid> DatasetIds { get; init; }

    /// <summary>
    /// Gets or sets the report ids.
    /// </summary>
    public HashSet<Guid> ReportIds { get; init; }
}
