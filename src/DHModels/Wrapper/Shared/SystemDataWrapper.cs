// -----------------------------------------------------------------------
// <copyright file="SystemDataWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
    using Newtonsoft.Json.Linq;
    using System;

    public class SystemDataWrapper : BaseEntityWrapper
    {
        private const string keyLastModifiedAt = "lastModifiedAt";
        private const string keyLastModifiedBy = "lastModifiedBy";
        private const string keyCreatedAt = "createdAt";
        private const string keyCreatedBy = "createdBy";

        public SystemDataWrapper(string createdBy, DateTime? createdAt = null) : base([])
        {
            var now = DateTime.UtcNow;
            this.CreatedBy = createdBy;
            this.LastModifiedBy = createdBy;
            this.CreatedAt = createdAt ?? now;
            this.LastModifiedAt = createdAt ?? now;
        }

        public SystemDataWrapper(JObject jObject) : base(jObject)
        {
        }

        public SystemDataWrapper() : base([])
        {
        }

        [EntityProperty(keyLastModifiedAt, true)]
        public DateTime? LastModifiedAt
        {
            get => this.GetPropertyValue<DateTime?>(keyLastModifiedAt);
            set => this.SetPropertyValue(keyLastModifiedAt, value);
        }

        [EntityProperty(keyLastModifiedBy, true)]
        public string LastModifiedBy
        {
            get => this.GetPropertyValue<string>(keyLastModifiedBy);
            set => this.SetPropertyValue(keyLastModifiedBy, value);
        }

        [EntityProperty(keyCreatedAt, true)]
        public DateTime? CreatedAt
        {
            get => this.GetPropertyValue<DateTime?>(keyCreatedAt);
            set => this.SetPropertyValue(keyCreatedAt, value?.GetDateTimeStr());
        }

        [EntityProperty(keyCreatedBy, true)]
        public string CreatedBy
        {
            get => this.GetPropertyValue<string>(keyCreatedBy);
            set => this.SetPropertyValue(keyCreatedBy, value);
        }

        public void OnModify(string modifiedBy, DateTime? modifiedAt = null)
        {
            this.LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            this.LastModifiedBy = modifiedBy;
        }
    }
}
