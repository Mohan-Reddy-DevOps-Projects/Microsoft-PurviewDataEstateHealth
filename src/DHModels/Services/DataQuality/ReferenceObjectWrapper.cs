// -----------------------------------------------------------------------
// <copyright file="ReferenceObjectWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Globalization;

    public enum ReferenceType
    {
        BusinessDomainReference,
        DataSourceReference,
        DataProductReference,
        DataAssetReference,
        ObserverReference,
        ProfileReference
    }

    public class ReferenceObjectWrapper : BaseEntityWrapper
    {
        private const string keyType = "type";
        private const string keyReferenceId = "referenceId";
        private const string keyReferenceName = "name";

        public static ReferenceObjectWrapper Create(JObject jObject)
        {
            return new ReferenceObjectWrapper(jObject);
        }

        public ReferenceObjectWrapper(JObject jObject) : base(jObject)
        {
        }

        public ReferenceObjectWrapper(ReferenceType type, string referenceId) : base(new JObject())
        {
            this.Type = type;
            this.ReferenceId = referenceId;
        }

        public ReferenceObjectWrapper(ReferenceType type, string referenceId, string referenceName) : base(new JObject())
        {
            this.Type = type;
            this.ReferenceId = referenceId;
            this.ReferenceName = referenceName;
        }

        [EntityProperty("type")]
        public ReferenceType Type
        {
            get
            {
                var typeStr = this.GetPropertyValue<string>(keyType);
                if (!Enum.TryParse(typeStr, out ReferenceType referenceType))
                {
                    throw new EntityValidationException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessageEnumPropertyValueNotValid,
                        typeStr,
                        keyType,
                        String.Join(", ", Enum.GetNames(typeof(ReferenceType)))));
                }
                return referenceType;
            }
            set => this.SetPropertyValue(keyType, value.ToString());
        }

        [EntityProperty("referenceId")]
        public string ReferenceId
        {
            get => this.GetPropertyValue<string>(keyReferenceId);
            set => this.SetPropertyValue(keyReferenceId, value);
        }

        [EntityProperty("name")]
        public string ReferenceName
        {
            get => this.GetPropertyValue<string>(keyReferenceName);
            set => this.SetPropertyValue(keyReferenceName, value);
        }
    }
}
