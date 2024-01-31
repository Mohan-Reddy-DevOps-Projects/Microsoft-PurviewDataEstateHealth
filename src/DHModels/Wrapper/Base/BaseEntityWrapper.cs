// -----------------------------------------------------------------------
// <copyright file="BaseEntityWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.ActiveGlossary.Models.Service.Base
{
    using Microsoft.Purview.ActiveGlossary.Models.Util;
    using Microsoft.Purview.ActiveGlossary.Models.Validators;
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    public class BaseEntityWrapper : JObjectWrapper
    {
        public BaseEntityWrapper(JObject jObject) : base(jObject) { }

        public T GetPropertyValueAsWrapper<T>(string key) where T : BaseEntityWrapper
        {
            var jObject = this.GetPropertyValue<JObject>(key);
            return EntityWrapperHelper.CreateEntityWrapper<T>(jObject);
        }

        public IEnumerable<T> GetPropertyValueAsWrappers<T>(string key) where T : BaseEntityWrapper
        {
            var jObjects = this.GetPropertyValues<JObject>(key);
            if (jObjects == null)
            {
                return null;
            }
            return jObjects.Select((jObject) => EntityWrapperHelper.CreateEntityWrapper<T>(jObject));
        }

        public void SetPropertyValueFromWrapper<T>(string key, T wrapper) where T : BaseEntityWrapper
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }
            this.SetPropertyValue(key, wrapper.JObject);
        }

        public void SetPropertyValueFromWrappers<T>(string key, IEnumerable<T> wrappers) where T : BaseEntityWrapper
        {
            if (wrappers == null)
            {
                this.SetPropertyValue(key, null);
                return;
            }

            var jObjects = wrappers.Select(ruleWrapper => ruleWrapper.JObject);
            this.SetPropertyValue(key, jObjects);
        }

        public virtual void Validate()
        {
            var props = this.GetType().GetProperties();

            // Check required container properties exist
            var requiredContainerPropertyNames = new HashSet<string>();
            foreach (var prop in props)
            {
                var entityPropAttribute = prop.GetCustomAttribute<BaseEntityPropertyAttribute>(true);
                if (entityPropAttribute != null)
                {
                    if (!String.IsNullOrEmpty(entityPropAttribute.ContainerPropertyName)
                        && prop.GetCustomAttribute<EntityRequiredValidatorAttribute>(true) != null)
                    {
                        requiredContainerPropertyNames.Add(entityPropAttribute.ContainerPropertyName);
                    }
                }
            }
            foreach (var requiredContainerPropertyName in requiredContainerPropertyNames)
            {
                if (!this.JObject.TryGetValue(requiredContainerPropertyName, PropertyNameComparison, out var containerValue)
                    || containerValue.Type == JTokenType.Null)
                {
                    throw new EntityValidationException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            StringResources.ErrorMessageValueRequired,
                            requiredContainerPropertyName));
                }
            }

            // Check every props
            foreach (var prop in props)
            {
                var entityPropAttribute = prop.GetCustomAttribute<BaseEntityPropertyAttribute>(true);
                if (entityPropAttribute != null)
                {
                    string propertyName = entityPropAttribute.PropertyName;

                    JObject targetObject;

                    if (String.IsNullOrEmpty(entityPropAttribute.ContainerPropertyName))
                    {
                        targetObject = this.JObject;
                    }
                    else
                    {
                        var containerObject = this.GetPropertyValue<JObject>(entityPropAttribute.ContainerPropertyName);
                        if (containerObject == null)
                        {
                            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageValueRequired, entityPropAttribute.ContainerPropertyName));
                        }
                        else
                        {
                            targetObject = containerObject;
                        }
                    }

                    if (!targetObject.TryGetValue(propertyName, PropertyNameComparison, out var jsonValue))
                    {
                        var attr = prop.GetCustomAttribute<EntityRequiredValidatorAttribute>(true);
                        if (attr != null)
                        {
                            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageValueRequired, propertyName));
                        }
                    }

                    if (jsonValue == null)
                    {
                        continue;
                    }

                    // Check point#1, expected type can match actual type in json
                    var expectedType = prop.PropertyType;
                    try
                    {
                        TypeUtils.IsAcceptableJsonValueType(expectedType, jsonValue);
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageTypeNotMatch, propertyName, expectedType.Name), ex);
                    }

                    // After passing type check, we can call GetValue safely
                    object value = this.GetPropertyValueByReflection(prop);
                    if (value is BaseEntityWrapper memberWrapper)
                    {
                        memberWrapper.Validate();
                    }
                    else if (value is IEnumerable<BaseEntityWrapper> enumerableMemberWrappers)
                    {
                        foreach (var enumerableMemberWrapper in enumerableMemberWrappers)
                        {
                            enumerableMemberWrapper.Validate();
                        }
                    }

                    // Check point#2, specific attribute rules
                    var validatorAttributes = prop.GetCustomAttributes<EntityValidatorAttribute>(true);
                    foreach (var validatorAttribute in validatorAttributes)
                    {
                        validatorAttribute.Validate(value, propertyName);
                    }
                }
            }
        }

        public virtual void NormalizeInput()
        {
            var validPropNames = new HashSet<string>();
            var props = this.GetType().GetProperties();

            foreach (var prop in props)
            {
                var entityPropAttribute = prop.GetCustomAttribute<EntityPropertyAttribute>(true);
                if (entityPropAttribute != null)
                {
                    if (!entityPropAttribute.IsReadOnly)
                    {
                        this.HandleNotReadOnlyPropForNormalizingInput(prop, entityPropAttribute.PropertyName, validPropNames);
                    }
                }
            }

            string[] unknownProps = this.JObject.Properties().Where(p => !validPropNames.Contains(p.Name)).Select(p => p.Name).ToArray();
            foreach (string prop in unknownProps)
            {
                _ = this.RemoveProperty(prop);
            }
        }

        public virtual BaseEntityWrapper GetEnumeratePayloadWrapper()
        {
            var result = EntityWrapperHelper.CreateEntityWrapper<BaseEntityWrapper>(new JObject());
            return result;
        }

        public Dictionary<string, List<string>> CollectReferences()
        {
            var result = new Dictionary<string, List<string>>();

            this.CollectReferencesInternal(result);

            return result;
        }

        protected void HandleNotReadOnlyPropForNormalizingInput(PropertyInfo propInfo, string propName, HashSet<string> validPropNames)
        {
            if (propInfo == null)
            {
                throw new ArgumentNullException(nameof(propInfo));
            }

            if (validPropNames == null)
            {
                throw new ArgumentNullException(nameof(validPropNames));
            }

            validPropNames.Add(propName);

            if (propInfo.PropertyType.IsSubclassOf(typeof(BaseEntityWrapper)) ||
                typeof(IEnumerable<BaseEntityWrapper>).IsAssignableFrom(propInfo.PropertyType))
            {
                object value = this.GetPropertyValueByReflection(propInfo);

                if (value is BaseEntityWrapper memberWrapper)
                {
                    memberWrapper.NormalizeInput();
                }
                else if (value is IEnumerable<BaseEntityWrapper> enumerableMemberWrappers)
                {
                    foreach (var enumerableMemberWrapper in enumerableMemberWrappers)
                    {
                        enumerableMemberWrapper.NormalizeInput();
                    }
                }
            }
        }

        private object GetPropertyValueByReflection(PropertyInfo propInfo)
        {
            object value;
            try
            {
                value = propInfo.GetValue(this);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException ?? e;
            }
            return value;
        }

        private void CollectReferencesInternal(Dictionary<string, List<string>> result)
        {
            var props = this.GetType().GetProperties();

            foreach (var propInfo in props)
            {
                if (propInfo.PropertyType.IsSubclassOf(typeof(BaseEntityWrapper)) ||
                typeof(IEnumerable<BaseEntityWrapper>).IsAssignableFrom(propInfo.PropertyType))
                {
                    object value = this.GetPropertyValueByReflection(propInfo);

                    if (value is BaseEntityWrapper memberWrapper)
                    {
                        memberWrapper.CollectReferencesInternal(result);
                    }
                    else if (value is IEnumerable<BaseEntityWrapper> enumerableMemberWrappers)
                    {
                        foreach (var enumerableMemberWrapper in enumerableMemberWrappers)
                        {
                            enumerableMemberWrapper.CollectReferencesInternal(result);
                        }
                    }
                }
            }
        }
    }
}
