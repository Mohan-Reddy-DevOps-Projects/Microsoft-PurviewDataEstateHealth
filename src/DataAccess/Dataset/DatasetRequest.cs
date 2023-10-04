// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Text.Json;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Dataset request.
/// </summary>
public sealed class DatasetRequest : IDatasetRequest
{
    /// <summary>
    /// Constructor
    /// </summary>
    public DatasetRequest() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="other"></param>
    public DatasetRequest(IDatasetRequest other)
    {
        this.AccountId = other.AccountId;
        this.ProfileId = other.ProfileId;
        this.WorkspaceId = other.WorkspaceId;
        this.DatasetName = other.DatasetName;
        this.DatabaseName = other.DatabaseName;
        this.DatabaseSchema = other.DatabaseSchema;
        this.Server = other.Server;
        this.DatasetContainer = other.DatasetContainer;
        this.DatasetFileName = other.DatasetFileName;
        this.DatasetId = other.DatasetId;
        this.PowerBICredential = other.PowerBICredential;
        this.OptimizedDataset = other.OptimizedDataset;
    }

    /// <inheritdoc/>
    public Guid AccountId { get; init; }

    /// <inheritdoc/>
    public Guid ProfileId { get; init; }

    /// <inheritdoc/>
    public Guid WorkspaceId { get; init; }

    /// <inheritdoc/>
    public string DatasetName { get; init; }

    /// <inheritdoc/>
    public string DatabaseName { get; init; }

    /// <inheritdoc/>
    public string DatabaseSchema { get; init; }

    /// <inheritdoc/>
    public string Server { get; init; }

    /// <inheritdoc/>
    public string DatasetContainer { get; init; }

    /// <inheritdoc/>
    public Guid DatasetId { get; init; }

    /// <inheritdoc/>
    public string DatasetFileName { get; init; }

    /// <inheritdoc/>
    public PowerBICredential PowerBICredential { get; init; }

    /// <inheritdoc/>
    public bool OptimizedDataset { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
