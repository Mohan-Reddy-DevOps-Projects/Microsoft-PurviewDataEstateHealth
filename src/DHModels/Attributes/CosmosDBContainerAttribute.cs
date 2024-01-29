#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class)]
public class CosmosDBContainerAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
