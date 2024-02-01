// -----------------------------------------------------------------------
// <copyright file="JObjectWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base
{
    using Microsoft.Purview.DataEstateHealth.DHModels;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Globalization;
    using System.Xml;

    public class JObjectWrapper : JObjectBaseWrapper
    {
        protected static string TimeSpanToString(TimeSpan time)
        {
            return XmlConvert.ToString(time);
        }

        protected static int GetRetryInterval(int retryInterval)
        {
            return retryInterval > 0 ? retryInterval : 30;
        }

        public string DefaultTimeout => this.GetStringTimeout(this.DefaultTimeoutTimeSpan);

        public JObjectWrapper(JObject jObject) : base(jObject)
        {
        }

        public override T GetPropertyValue<T>(string key)
        {
            if (this.JObject != null && this.JObject.TryGetValue(key, PropertyNameComparison, out var token))
            {
                if (token is JValue && ((JValue)token).Value == null)
                {
                    return default;
                }

                try
                {
                    if (typeof(T) == typeof(TimeSpan))
                    {
                        string timeSpantoString = token.ToString();
                        var timeSpanConvertResult = TimeSpanUtils.ConvertToInvariantTimeSpan(timeSpantoString);
                        token = (JToken)timeSpanConvertResult;
                    }

                    if (typeof(T) == typeof(JObject))
                    {
                        return token.Value<T>();
                    }

                    return token.ToObject<T>();
                }

                // TBD:  fill the message
                catch (ArgumentException argumentException)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageEntityParseFailed), argumentException);
                }
                catch (FormatException formatException)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageEntityParseFailed), formatException);
                }
                catch (InvalidCastException invalidCastException)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageEntityParseFailed), invalidCastException);
                }
                catch (JsonException jsonCastException)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageEntityParseFailed), jsonCastException);
                }
                catch (Exception exception)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageEntityParseFailed), exception);
                }
            }

            return default;
        }

        protected virtual TimeSpan DefaultTimeoutTimeSpan => new TimeSpan(7, 0, 0, 0);

        public bool IsProperty<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || this.JObject == null)
            {
                return false;
            }

            var value = this.GetPropertyValue<object>(key);
            return value is T;
        }

        protected string GetStringTimeout(TimeSpan timeout)
        {
            string timeoutString = TimeSpanToString(timeout);
            if (timeout == TimeSpan.Zero)
            {
                timeoutString = TimeSpanToString(this.DefaultTimeoutTimeSpan);
            }
            return timeoutString;
        }
    }
}