// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataAccess.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Newtonsoft.Json;

/// <summary>
/// Base class for data estate health resource entities that will be persisted in Artifact Store service.
/// </summary>
internal abstract class ArtifactStoreEntity
{
    /// <summary>
    /// Object id of the entity
    /// </summary>
    [JsonProperty("objectId")]
    public Guid ObjectId { get; set; }

    /// <summary>
    /// Version of the entity
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    /// Date/Time this entity was created.
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date/Time this entity was modified.
    /// </summary>
    [JsonProperty("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Get the type of entity.
    /// </summary>
    public abstract DataEstateHealthEntityTypes GetEntityType();

    /// <inheritdoc />
    public abstract Dictionary<string, object> GetIndexedProperties();

    /// <summary>
    /// Validate payload to be persisted.
    /// </summary>
    public virtual void Validate(ICommonFieldValidationService fieldValidationService)
    {
        //TODO 2836281: Fill out entity validation
    }
}
