// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <summary>
/// Entity model.
/// </summary>
public interface IEntityModel
{
    /// <summary>
    /// Account for the entity.
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Etag for the entity.
    /// </summary>
    /// <remarks>
    /// Read behavior:
    /// * All entities read or returned from a write will have an Etag
    /// Write behavior:
    /// * If null (default behavior for new entities): Entity must not already exist
    /// * If string.Empty: Entity should not do etag checks
    /// * Otherwise (default for entities that have been read): Entity's etag must match
    /// Overall these behaviors do the right thing by default.
    /// </remarks>
    string Etag { get; set; }

    /// <summary>
    /// Identifier of the entity
    /// </summary>
    /// <remarks>
    /// There is nothing particularly special about this field, it's just rather common. The actual Id used for lookups
    /// is whatever is chosen in GetEntityId which sometimes includes this field.
    /// </remarks>
    Guid Id { get; }

    /// <summary>
    /// Name of the entity
    /// </summary>
    /// <remarks>
    /// There is nothing particularly special about this field, it's just rather common. The actual Id used for lookups
    /// is whatever is chosen in GetEntityId which sometimes includes this field.
    /// </remarks>
    string Name { get; }
}
