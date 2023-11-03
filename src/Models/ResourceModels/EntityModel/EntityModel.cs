// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using global::Azure;

/// <summary>
/// Entity model.
/// </summary>
public abstract class EntityModel : IEntityModel
{
    /// <inheritdoc />
    public string RowKey { get; set; }

    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <inheritdoc />
    public string PartitionKey { get; set; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public ETag ETag { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    protected EntityModel()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="id"></param>
    /// <param name="rowKey"></param>
    /// <param name="etag"></param>
    protected EntityModel(string partitionKey, Guid id, string rowKey, ETag etag)
    {
        this.PartitionKey = partitionKey;
        this.Id = id;
        this.RowKey = rowKey;
        this.ETag = etag;
    }
}
