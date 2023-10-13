// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// The dataset request.
/// </summary>
public interface IDatasetRequest
{
    /// <summary>
    /// The profile id.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// The workspace id.
    /// </summary>
    Guid WorkspaceId { get; }

    /// <summary>
    /// The dataset name.
    /// </summary>
    string DatasetName { get; }

    /// <summary>
    /// The dataset id.
    /// </summary>
    Guid DatasetId { get; }

    /// <summary>
    /// The dataset container.
    /// </summary>
    string DatasetContainer { get; }

    /// <summary>
    /// The dataset file name.
    /// </summary>
    string DatasetFileName { get; }

    /// <summary>
    /// The credential used to access the dataset.
    /// </summary>
    PowerBICredential PowerBICredential { get; }

    /// <summary>
    /// Whether the dataset is in optimized formated for large sizes.
    /// </summary>
    bool OptimizedDataset { get; }

    /// <summary>
    /// Whether to skip the report creation.
    /// </summary>
    bool SkipReport { get; }

    /// <summary>
    /// Parameters to be used in the dataset creation.
    /// </summary>
    Dictionary<string, string> Parameters { get; }
}
