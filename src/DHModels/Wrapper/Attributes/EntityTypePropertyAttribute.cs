// -----------------------------------------------------------------------
// <copyright file="EntityTypePropertyAttribute.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes
{
    using Microsoft.Purview.ActiveGlossary.Models.Service.Base;
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityTypePropertyAttribute : BaseEntityPropertyAttribute
    {
        public EntityTypePropertyAttribute(string propertyName, bool isReadOnly = false)
            : base(propertyName, isReadOnly, DynamicEntityWrapper.keyTypeProperties)
        {
        }
    }
}
