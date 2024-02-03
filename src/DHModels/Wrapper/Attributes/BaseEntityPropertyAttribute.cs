// -----------------------------------------------------------------------
// <copyright file="BaseEntityPropertyAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class BaseEntityPropertyAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public bool IsReadOnly { get; set; }

        public string ContainerPropertyName { get; set; }

        protected BaseEntityPropertyAttribute(string propertyName, bool isReadOnly, string containerPropertyName = null)
        {
            this.PropertyName = propertyName;
            this.IsReadOnly = isReadOnly;
            this.ContainerPropertyName = containerPropertyName;
        }
    }
}
