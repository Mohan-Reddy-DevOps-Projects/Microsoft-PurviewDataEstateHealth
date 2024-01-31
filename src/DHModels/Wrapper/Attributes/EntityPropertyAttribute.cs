// -----------------------------------------------------------------------
// <copyright file="EntityPropertyAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityPropertyAttribute : BaseEntityPropertyAttribute
    {
        public EntityPropertyAttribute(string propertyName, bool isReadOnly = false) : base(propertyName, isReadOnly) { }
    }
}
