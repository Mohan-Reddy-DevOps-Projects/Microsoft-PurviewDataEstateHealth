// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// User data store request for SAS.
/// </summary>
public class StorageSasRequest
{
    /// <summary>
    /// Gets or sets the signed permissions for the account SAS.
    /// Possible values include: Read (r), Write (w), Delete (d), List (l), Add (a), Create (c), Update (u) and Process (p). Possible values include: 'r', 'd', 'w', 'l', 'a', 'c', 'u', 'p'.
    /// </summary>
    /// <value>
    /// The permissions.
    /// </value>
    public string Permissions { get; set; }

    /// <summary>
    /// Gets or sets the time to live of the SAS token
    /// </summary>
    /// <value>
    /// The time to live.
    /// </value>
    public TimeSpan TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the blob path
    /// </summary>
    /// <value>
    /// The Blob Path.
    /// </value>
    public string Path { get; set; } = string.Empty;

    public string Resource { get; set; } = "d";

    public bool? IsDirectory { get; set; } = true;
}
