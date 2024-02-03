namespace Microsoft.Purview.DataEstateHealth.DHModels.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class CosmosDBPartitionKeyAttribute : Attribute
{
    // This is a marker attribute, so no additional properties or methods are needed.
}