#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.AttributeHandlers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using System;
using System.Linq;
using System.Reflection;

internal static class CosmosDBAttributeHandlers
{
    internal static void HandleCosmosDBContainerAttribute(ModelBuilder modelBuilder)
    {
        // Use reflection to get all entity types with the Container attribute
        var entityTypes = modelBuilder.Model.GetEntityTypes().Where(t => t.ClrType.GetCustomAttributes(typeof(CosmosDBContainerAttribute), true).Any());

        foreach (var type in entityTypes)
        {
            var containerAttribute = type.ClrType.GetCustomAttribute<CosmosDBContainerAttribute>();
            if (containerAttribute != null)
            {
                modelBuilder.Entity(type.ClrType, builder =>
                {
                    builder.ToContainer(containerAttribute.Name);
                });
            }
        }
    }

    internal static void HandleCosmosDBEnumStringAttribute(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var propertyInfo = property.PropertyInfo;
                if (propertyInfo != null)
                {
                    var hasAttribute = propertyInfo.GetCustomAttribute<CosmosDBEnumStringAttribute>() != null;
                    if (hasAttribute && propertyInfo.PropertyType.IsEnum)
                    {
                        // Use reflection to create an instance of EnumToStringConverter for the specific enum type
                        var converterType = typeof(EnumToStringConverter<>).MakeGenericType(propertyInfo.PropertyType);
                        var converter = Activator.CreateInstance(converterType) as ValueConverter;

                        property.SetValueConverter(converter);
                    }
                    else if (hasAttribute)
                    {
                        throw new InvalidOperationException($"The CosmosDBEnumString attribute can only be applied to enum properties. Property '{propertyInfo.Name}' in '{entityType.Name}' is not an enum.");
                    }
                }
            }
        }
    }
}
