#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Attributes;

using System;

[AttributeUsage(AttributeTargets.Property)]
public class CosmosDBEnumStringAttribute : Attribute
{
    // This is a marker attribute, so it's empty
}
