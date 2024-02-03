// -----------------------------------------------------------------------
// <copyright file="ObserverWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System;

    public class ObserverWrapper : DynamicEntityWrapper
    {
        private const string keyId = "id";
        private const string keyName = "name";
        private const string keyDescription = "description";
        private const string keyBusinessDomain = "businessDomain";
        private const string keyDataProduct = "dataProduct";
        private const string keyDataAsset = "dataAsset";

        private const string keyCreatedAt = "createdAt";
        private const string keyCreatedBy = "createdBy";
        private const string keyLastModifiedAt = "lastModifiedAt";
        private const string keyLastModifiedBy = "lastModifiedBy";
        private const string keyExecutionData = "executionData";
        private const string keyIsDeleted = "isDeleted";

        public enum ObserverType
        {
            Basic
        }

        public static ObserverWrapper Create(JObject obj)
        {
            return EntityWrapperHelper.CreateEntityWrapper<ObserverWrapper>(EntityCategory.Observer, EntityWrapperHelper.GetEntityType(obj), obj);
        }

        public ObserverWrapper(JObject jObject) : base(jObject) { }

        [EntityProperty(keyId)]
        [EntityRequiredValidator]
        [EntityIdValidator]
        public string Id
        {
            get => this.GetPropertyValue<string>(keyId);
            set => this.SetPropertyValue(keyId, value);
        }

        [EntityProperty(keyName)]
        [EntityRequiredValidator]
        [EntityNameValidator]
        public string Name
        {
            get => this.GetPropertyValue<string>(keyName);
            set => this.SetPropertyValue(keyName, value);
        }

        [EntityProperty(keyDescription)]
        public string Description
        {
            get => this.GetPropertyValue<string>(keyDescription);
            set => this.SetPropertyValue(keyDescription, value);
        }

        private ReferenceObjectWrapper businessDomain;

        [EntityProperty(keyBusinessDomain)]
        [EntityRequiredValidator]
        public ReferenceObjectWrapper BusinessDomain
        {
            get
            {
                this.businessDomain ??= this.GetPropertyValueAsWrapper<ReferenceObjectWrapper>(keyBusinessDomain);
                return this.businessDomain;
            }

            set
            {
                this.SetPropertyValueFromWrapper(keyBusinessDomain, value);
                this.businessDomain = value;
            }
        }

        private ReferenceObjectWrapper dataProduct;

        [EntityProperty(keyDataProduct)]
        [EntityRequiredValidator]
        public ReferenceObjectWrapper DataProduct
        {
            get
            {
                this.dataProduct ??= this.GetPropertyValueAsWrapper<ReferenceObjectWrapper>(keyDataProduct);
                return this.dataProduct;
            }

            set
            {
                this.SetPropertyValueFromWrapper(keyDataProduct, value);
                this.dataProduct = value;
            }
        }

        private ReferenceObjectWrapper dataAsset;

        [EntityProperty(keyDataAsset)]
        [EntityRequiredValidator]
        public ReferenceObjectWrapper DataAsset
        {
            get
            {
                this.dataAsset ??= this.GetPropertyValueAsWrapper<ReferenceObjectWrapper>(keyDataAsset);
                return this.dataAsset;
            }

            set
            {
                this.SetPropertyValueFromWrapper(keyDataAsset, value);
                this.dataAsset = value;
            }
        }

        [EntityProperty(keyLastModifiedAt, true)]
        public DateTime? LastModifiedAt
        {
            get => this.GetPropertyValue<DateTime?>(keyLastModifiedAt);
            private set => this.SetPropertyValue(keyLastModifiedAt, value?.GetDateTimeStr());
        }

        [EntityProperty(keyLastModifiedBy, true)]
        public string LastModifiedBy
        {
            get => this.GetPropertyValue<string>(keyLastModifiedBy);
            private set => this.SetPropertyValue(keyLastModifiedBy, value);
        }

        [EntityProperty(keyCreatedAt, true)]
        public DateTime? CreatedAt
        {
            get => this.GetPropertyValue<DateTime?>(keyCreatedAt);
            private set => this.SetPropertyValue(keyCreatedAt, value?.GetDateTimeStr());
        }

        [EntityProperty(keyCreatedBy, true)]
        public string CreatedBy
        {
            get => this.GetPropertyValue<string>(keyCreatedBy);
            private set => this.SetPropertyValue(keyCreatedBy, value);
        }

        [EntityProperty(keyExecutionData, true)]
        public JObject ExecutionData
        {
            get => this.GetPropertyValue<JObject>(keyExecutionData);
            private set => this.SetPropertyValue(keyExecutionData, value);
        }

        [EntityProperty(keyIsDeleted, true)]
        public bool IsDeleted
        {
            get => this.GetPropertyValue<bool>(keyIsDeleted);
            private set => this.SetPropertyValue(keyIsDeleted, value);
        }

        public override BaseEntityWrapper GetEnumeratePayloadWrapper()
        {
            var result = base.GetEnumeratePayloadWrapper();
            result.SetPropertyValue(keyName, this.Name);
            result.SetPropertyValue(keyId, this.Id);
            result.SetPropertyValue(keyDescription, this.Description);
            result.SetPropertyValueFromWrapper(keyBusinessDomain, this.BusinessDomain);
            result.SetPropertyValueFromWrapper(keyDataProduct, this.DataProduct);
            result.SetPropertyValueFromWrapper(keyDataAsset, this.DataAsset);
            result.SetPropertyValue(keyType, ObserverType.Basic.ToString());
            result.SetPropertyValue(keyLastModifiedAt, this.LastModifiedAt?.GetDateTimeStr());
            result.SetPropertyValue(keyLastModifiedBy, this.LastModifiedBy);
            result.SetPropertyValue(keyExecutionData, this.ExecutionData);
            result.SetPropertyValue(keyIsDeleted, this.IsDeleted);
            return result;
        }

        public virtual void OnCreate(string createdBy, DateTime? createdAt = null)
        {
            var now = createdAt ?? DateTime.UtcNow;
            this.CreatedAt = now;
            this.LastModifiedAt = now;
            this.CreatedBy = createdBy;
            this.LastModifiedBy = createdBy;
            this.IsDeleted = false;
        }

        public void OnModify(string modifiedBy, DateTime? modifiedAt = null)
        {
            this.LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            this.LastModifiedBy = modifiedBy;
        }

        public void OnReplace(ObserverWrapper existed)
        {
            Ensure.IsNotNull(existed, nameof(existed));

            this.CreatedAt = existed.CreatedAt;
            this.CreatedBy = existed.CreatedBy;
            this.Id = existed.Id;
        }

        public void OnSoftDelete(string modifiedBy, DateTime? modifiedAt = null)
        {
            this.LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            this.LastModifiedBy = modifiedBy;
            this.IsDeleted = true;
        }

        public void UpdateExecutionData(JObject executionData)
        {
            this.ExecutionData = executionData;
        }
    }
}
