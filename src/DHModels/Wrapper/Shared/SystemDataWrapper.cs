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
        protected const string KeyLastModifiedAt = "lastModifiedAt";
        protected const string KeyLastModifiedBy = "lastModifiedBy";
        protected const string KeyCreatedAt = "createdAt";
        protected const string KeyCreatedBy = "createdBy";

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

        [EntityProperty(KeyLastModifiedAt, true)]
        public virtual DateTime? LastModifiedAt
        {
            get => this.GetPropertyValue<DateTime?>(KeyLastModifiedAt);
            set => this.SetPropertyValue(KeyLastModifiedAt, value);
        }

        [EntityProperty(KeyLastModifiedBy, true)]
        public virtual string LastModifiedBy
        {
            get => this.GetPropertyValue<string>(KeyLastModifiedBy);
            set => this.SetPropertyValue(KeyLastModifiedBy, value);
        }

        [EntityProperty(KeyCreatedAt, true)]
        public virtual DateTime? CreatedAt
        {
            get => this.GetPropertyValue<DateTime?>(KeyCreatedAt);
            set => this.SetPropertyValue(KeyCreatedAt, value?.GetDateTimeStr());
        }

        [EntityProperty(KeyCreatedBy, true)]
        public virtual string CreatedBy
        {
            get => this.GetPropertyValue<string>(KeyCreatedBy);
            set => this.SetPropertyValue(KeyCreatedBy, value);
        }

        public void OnModify(string modifiedBy, DateTime? modifiedAt = null)
        {
            this.LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            this.LastModifiedBy = modifiedBy;
        }
    }
}
