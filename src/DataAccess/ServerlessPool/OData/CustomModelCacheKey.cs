// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.EntityFrameworkCore;
using System;

/// <summary>
/// A key that uniquely identifies the model for a given context and customer.
/// </summary>
internal class CustomModelCacheKey
{
    private readonly Type dbContextType;
    private readonly bool designTime;
    private readonly string schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomModelCacheKey" /> class.
    /// </summary>
    public CustomModelCacheKey(DbContext context, bool designTime)
    {
        this.dbContextType = context.GetType();
        this.schema = this.dbContextType == typeof(SynapseSqlContext) ? ((SynapseSqlContext)context).databaseSchema : null;
        this.designTime = designTime;
    }

    /// <summary>
    /// Determines if this key is equivalent to a given key
    /// </summary>
    protected virtual bool Equals(CustomModelCacheKey other)
    {
        return this.dbContextType == other.dbContextType
                && this.designTime == other.designTime
                && this.schema == other.schema;
    }

    /// <summary>
    /// Determines if this key is equivalent to a given object
    /// </summary>
    public override bool Equals(object obj) => (obj is CustomModelCacheKey otherAsKey) && this.Equals(otherAsKey);

    /// <summary>
    /// Gets the hash code for the key.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(this.dbContextType);
        hash.Add(this.designTime);
        hash.Add(this.schema);
        return hash.ToHashCode();
    }
}
