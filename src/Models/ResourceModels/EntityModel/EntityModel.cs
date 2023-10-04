// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <summary>
/// Entity model.
/// </summary>
public abstract class EntityModel : IEntityModel
{
    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <inheritdoc />
    public Guid AccountId { get; set; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public string Etag { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    protected EntityModel()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="etag"></param>
    protected EntityModel(Guid accountId, Guid id, string name, string etag)
    {
        this.AccountId = accountId;
        this.Id = id;
        this.Name = name;
        this.Etag = etag;
    }
}
