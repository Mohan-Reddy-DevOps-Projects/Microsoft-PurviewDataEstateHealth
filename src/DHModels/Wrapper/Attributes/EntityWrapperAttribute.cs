// -----------------------------------------------------------------------
// <copyright file="EntityWrapperAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    public enum EntityCategory
    {
        Observer,
        DatasetLocation,
        DatasetProjectAsItem,
        DatasetSchemaItem,
        Dataset,
        Rule,
        Control,
        Assessment,
        Action,
        Schedule,
        StatusPalette
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityWrapperAttribute : Attribute
    {
#pragma warning disable SA1000 // Keywords should be spaced correctly
        private static readonly object Locker = new();
#pragma warning restore SA1000 // Keywords should be spaced correctly

        public EntityCategory EntityCategory { get; internal set; }

        public string EntityType { get; internal set; }

        public EntityWrapperAttribute(string entityType, EntityCategory entityCategory)
        {
            this.EntityCategory = entityCategory;
            this.EntityType = entityType;
        }

        public EntityWrapperAttribute(EntityCategory entityCategory)
        {
            this.EntityCategory = entityCategory;
        }

        private static Dictionary<EntityCategory, Dictionary<string, Type>> wrapperTypes;

        public static Dictionary<EntityCategory, Dictionary<string, Type>> AllWrapperTypes
        {
            get
            {
                lock (Locker)
                {
                    if (wrapperTypes == null)
                    {
                        FetchAllWrappers();
                    }
                    return wrapperTypes;
                }
            }
        }

        public static Type GetWrapperType(EntityCategory category, string entityType)
        {
            Type type;
            try
            {
                AllWrapperTypes[category].TryGetValue(entityType, out type);
            }
            catch (KeyNotFoundException)
            {
                // in some extreme cases, the GetExecutingAssembly() won't return all types
                // fetch again to avoid runtime class load transient failure.
                FetchAllWrappers();
                AllWrapperTypes[category].TryGetValue(entityType, out type);
            }

            if (type == null)
            {
                throw new EntityValidationException(String.Format(
                    CultureInfo.InvariantCulture,
                    StringResources.ErrorMessageEnumPropertyValueNotValid,
                    entityType,
                    DynamicEntityWrapper.keyType,
                    String.Join(", ", AllWrapperTypes[category].Keys)));
            }
            return type;
        }

        private static void FetchAllWrappers()
        {
            lock (Locker)
            {
                var successor = new Dictionary<EntityCategory, Dictionary<string, Type>>();
                foreach (var type in typeof(EntityWrapperAttribute).Assembly.GetTypes())
                {
                    if (IsDefined(type, typeof(EntityWrapperAttribute)))
                    {
                        var attr = type.GetCustomAttribute<EntityWrapperAttribute>();
                        if (!string.IsNullOrEmpty(attr.EntityType))
                        {
                            successor.TryAdd(attr.EntityCategory, new Dictionary<string, Type>());
                            successor[attr.EntityCategory].TryAdd(attr.EntityType, type);
                        }
                    }
                }
                wrapperTypes = successor;
            }
        }
    }
}
