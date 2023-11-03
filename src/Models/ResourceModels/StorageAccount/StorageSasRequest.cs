// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// User data store request for SAS.
/// </summary>
public class StorageSasRequest
{
    /// <summary>
    /// Gets or sets the signed services accessible with the account SAS.
    /// Possible values include: Blob (b), Queue (q), Table (t), File (f). Possible values include: 'b', 'q', 't', 'f'
    /// </summary>
    /// <value>
    /// The services.
    /// </value>
    [JsonProperty("services")]
    public string Services { get; set; }

    /// <summary>
    /// Gets or sets the signed permissions for the account SAS.
    /// Possible values include: Read (r), Write (w), Delete (d), List (l), Add (a), Create (c), Update (u) and Process (p). Possible values include: 'r', 'd', 'w', 'l', 'a', 'c', 'u', 'p'.
    /// </summary>
    /// <value>
    /// The permissions.
    /// </value>
    [JsonProperty("permissions")]
    public string Permissions { get; set; }

    /// <summary>
    /// Gets or sets the time to live of the SAS token
    /// </summary>
    /// <value>
    /// The time to live.
    /// </value>
    [JsonProperty("timeToLive")]
    public TimeSpan TimeToLive { get; set; }

    /// <summary>
    /// Gets or sets the Blob Path
    /// </summary>
    /// <value>
    /// The Blob Path.
    /// </value>
    [JsonProperty("blobPath")]
    public string BlobPath { get; set; }
}
