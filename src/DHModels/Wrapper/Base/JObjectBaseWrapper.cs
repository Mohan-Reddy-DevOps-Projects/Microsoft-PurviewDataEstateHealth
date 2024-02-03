// -----------------------------------------------------------------------
// <copyright file="JObjectBaseWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class JObjectBaseWrapper
    {
        public const StringComparison PropertyNameComparison = StringComparison.OrdinalIgnoreCase;

        [NotMapped]
        public JObject JObject { get; private set; }

        public JObjectBaseWrapper(JObject jObject)
        {
            this.JObject = jObject ?? [];
        }

        public virtual T GetPropertyValue<T>(string key)
        {
            if (this.JObject != null && this.JObject.TryGetValue(key, PropertyNameComparison, out var token))
            {
                if (token is JValue && ((JValue)token).Value == null)
                {
                    return default;
                }

                try
                {
                    return token.ToObject<T>();
                }
                catch (FormatException)
                {
                    throw;
                }
                catch (InvalidCastException)
                {
                    throw;
                }
            }

            return default;
        }

        public void SetOrRemovePropertyValue(string key, object value, object valueToRemove = null)
        {
            if (value == valueToRemove)
            {
                this.RemoveProperty(key);
            }
            else
            {
                this.SetPropertyValue(key, value);
            }
        }

        public void SetPropertyValue(string key, object value)
        {
            if (this.JObject.TryGetValue(key, PropertyNameComparison, out var token))
            {
                token.Parent.Remove();
            }

            if (value != null)
            {
                // To keep the same reference
                if (value is JToken jTokenValue)
                {
                    this.JObject[key] = jTokenValue;
                }
                else if (value is IEnumerable<JObject> jObjects)
                {
                    this.JObject[key] = new JArray(jObjects);
                }
                else
                {
                    this.JObject[key] = JToken.FromObject(value);
                }
            }
        }

        public void SetPropertyValueFromEnumerable(string key, IEnumerable<JObject> entityJObjects)
        {
            if (entityJObjects == null)
            {
                throw new ArgumentNullException(nameof(entityJObjects));
            }

            var entitiesJArray = new JArray();
            foreach (var jObject in entityJObjects)
            {
                entitiesJArray.Add(jObject);
            }
            this.SetPropertyValue(key, entitiesJArray);
        }

        public void SetPropertyValueFromDictionary(string key, IDictionary<string, JObject> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            var value = new JObject();
            foreach (var keyValuePair in dictionary)
            {
                value[keyValuePair.Key] = keyValuePair.Value;
            }
            this.SetPropertyValue(key, value);
        }

        public IEnumerable<T> GetPropertyValues<T>(string key)
        {
            if (this.JObject.TryGetValue(key, PropertyNameComparison, out var token))
            {
                var container = token as JContainer;
                return container.Values<T>();
            }

            return default;
        }

        public IDictionary<string, JObject> GetPropertyValueDictionary(string key)
        {
            var value = this.GetPropertyValue<JObject>(key);
            if (value == null)
            {
                return null;
            }

            IDictionary<string, JObject> dictionary = new Dictionary<string, JObject>();
            foreach (var keyValuePair in value)
            {
                var jObject = keyValuePair.Value as JObject;
                dictionary[keyValuePair.Key] = jObject;
            }

            return dictionary;
        }

        public bool RemoveProperty(string key)
        {
            if (this.JObject.TryGetValue(key, PropertyNameComparison, out var token))
            {
                token.Parent.Remove();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}