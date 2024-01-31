// -----------------------------------------------------------------------
// <copyright file="DynamicEntityWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.ActiveGlossary.Models.Service.Base
{
    using Microsoft.Purview.ActiveGlossary.Models.Validators;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class DynamicEntityWrapper : BaseEntityWrapper
    {
        public const string keyType = "type";
        public const string keyTypeProperties = "typeProperties";

        public DynamicEntityWrapper(JObject jObject) : base(jObject) { }

        [EntityProperty(keyType)]
        [EntityRequiredValidator]
        public string Type => this.GetPropertyValue<string>(keyType);

        private BaseEntityWrapper typePropertiesWrapper;

        public BaseEntityWrapper TypePropertiesWrapper
        {
            get
            {
                this.typePropertiesWrapper ??= this.GetPropertyValueAsWrapper<BaseEntityWrapper>(keyTypeProperties);
                return this.typePropertiesWrapper;
            }

            set
            {
                this.SetPropertyValueFromWrapper(keyTypeProperties, value);
                this.typePropertiesWrapper = value;
            }
        }

        [EntityProperty(keyTypeProperties)]
        public JObject TypeProperties
        {
            get => this.GetPropertyValue<JObject>(keyTypeProperties);
            set => this.SetPropertyValue(keyTypeProperties, value);
        }

        public T GetTypePropertyValue<T>(string key)
        {
            return this.TypeProperties == null ? default : this.TypePropertiesWrapper.GetPropertyValue<T>(key);
        }

        public void SetTypePropertyValue<T>(string key, T value)
        {
            this.TypePropertiesWrapper.SetPropertyValue(key, value);
        }

        public T GetTypePropertyValueAsWrapper<T>(string key) where T : BaseEntityWrapper
        {
            var jObject = this.GetTypePropertyValue<JObject>(key);
            return EntityWrapperHelper.CreateEntityWrapper<T>(jObject);
        }

        public IEnumerable<T> GetTypePropertyValues<T>(string key)
        {
            return this.TypeProperties == null ? default : this.TypePropertiesWrapper.GetPropertyValues<T>(key);
        }

        public IEnumerable<T> GetTypePropertyValueAsWrappers<T>(string key) where T : BaseEntityWrapper
        {
            var jObjects = this.GetTypePropertyValues<JObject>(key);
            var results = new List<T>();
            foreach (var jObject in jObjects)
            {
                results.Add(EntityWrapperHelper.CreateEntityWrapper<T>(jObject));
            }
            return results;
        }

        public void SetTypePropertyValueFromWrapper<T>(string key, T wrapper) where T : BaseEntityWrapper
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }
            this.SetTypePropertyValue(key, wrapper.JObject);
        }

        public void SetTypePropertyValueFromWrappers<T>(string key, IEnumerable<T> wrappers) where T : BaseEntityWrapper
        {
            var jObjects = wrappers.Select(ruleWrapper => ruleWrapper.JObject);
            this.SetTypePropertyValue(key, jObjects);
        }

        public override void NormalizeInput()
        {
            base.NormalizeInput();

            if (this.TypeProperties != null)
            {
                var validPropNames = new HashSet<string>();
                var props = this.GetType().GetProperties();

                foreach (var prop in props)
                {
                    var entityPropAttribute = prop.GetCustomAttribute<EntityTypePropertyAttribute>(true);
                    if (entityPropAttribute != null)
                    {
                        if (!entityPropAttribute.IsReadOnly)
                        {
                            this.HandleNotReadOnlyPropForNormalizingInput(prop, entityPropAttribute.PropertyName, validPropNames);
                        }
                    }
                }

                string[] unknownProps = this.TypeProperties.Properties().Where(p => !validPropNames.Contains(p.Name)).Select(p => p.Name).ToArray();
                foreach (string prop in unknownProps)
                {
                    _ = this.TypePropertiesWrapper.RemoveProperty(prop);
                }
            }
        }
    }
}
