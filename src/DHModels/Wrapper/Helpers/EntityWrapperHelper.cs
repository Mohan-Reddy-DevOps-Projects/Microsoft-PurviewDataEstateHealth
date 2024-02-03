// -----------------------------------------------------------------------
// <copyright file="EntityWrapperHelper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Globalization;
    using System.Reflection;

    public static class EntityWrapperHelper
    {
        public static string GetEntityType(JObject entityObject)
        {
            if (entityObject == null)
            {
                throw new ArgumentNullException(nameof(entityObject));
            }

            if (entityObject.TryGetValue("type", out var type))
            {
                return (string)type;
            }
            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageValueRequired, DynamicEntityWrapper.keyType));
        }

        public static T CreateEntityWrapper<T>(EntityCategory category, string entityType, JObject obj) where T : BaseEntityWrapper
        {
            try
            {
                var type = EntityWrapperAttribute.GetWrapperType(category, entityType);
                return type.GetConstructor(new[] { typeof(JObject) }).Invoke(new object[] { obj }) as T;
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }

        public static T CreateEntityWrapper<T>(JObject jObject) where T : BaseEntityWrapper
        {
            if (jObject == null)
            {
                return null;
            }

            try
            {
                var type = typeof(T);
                var createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                return createMethod != null ? (T)createMethod.Invoke(null, new[] { jObject }) : (T)Activator.CreateInstance(type, new[] { jObject });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
        }
    }
}
