// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;

using global::Azure.Data.Tables;

internal abstract class TableEntity : Models.EntityModel, ITableEntity
{
    /// <summary>
    /// The Timestamp property is a DateTimeOffset value that is maintained on the server side to record the time an entity was last modified.
    /// The Table service uses the Timestamp property internally to provide optimistic concurrency.
    /// The value of Timestamp is a monotonically increasing value, meaning that each time the entity is modified, the value of Timestamp increases for that entity.
    /// This property should not be set on insert or update operations (the value will be ignored).
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }
}
