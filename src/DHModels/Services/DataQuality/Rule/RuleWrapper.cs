// -----------------------------------------------------------------------
// <copyright file="RuleWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System;

    public enum RuleStatus
    {
        Active,
        Disabled,
    }

    public class RuleWrapper : DynamicEntityWrapper
    {
        public const string KeyColumn = "column";
        public const string KeyColumns = "columns";

        private const string keyId = "id";
        private const string keyName = "name";
        private const string KeySuggested = "suggested";
        private const string keyDescription = "description";
        private const string keyStatus = "status";

        private const string keyAnnotations = "annotations";
        private const string keyCreatedAt = "createdAt";
        private const string keyCreatedBy = "createdBy";
        private const string keyLastModifiedAt = "lastModifiedAt";
        private const string keyLastModifiedBy = "lastModifiedBy";

        private const string keyDimension = "dimension";

        public RuleWrapper(JObject jObject) : base(jObject) { }

        public static RuleWrapper Create(JObject obj)
        {
            return EntityWrapperHelper.CreateEntityWrapper<RuleWrapper>(EntityCategory.Rule, EntityWrapperHelper.GetEntityType(obj), obj);
        }

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

        [EntityProperty(KeySuggested)]
        public string Suggested
        {
            get => this.GetPropertyValue<string>(KeySuggested);
            set => this.SetPropertyValue(KeySuggested, value);
        }

        [EntityProperty(keyDescription)]
        public string Description
        {
            get => this.GetPropertyValue<string>(keyDescription);
            set => this.SetPropertyValue(keyDescription, value);
        }

        [EntityProperty(keyStatus)]
        [EntityRequiredValidator]
        public RuleStatus Status
        {
            get
            {
                var statusStr = this.GetPropertyValue<string>(keyStatus);
                return Enum.TryParse<RuleStatus>(statusStr, true, out var status) ? status : RuleStatus.Disabled;
            }
            set => this.SetPropertyValue(keyStatus, value.ToString());
        }

        [EntityProperty(keyAnnotations)]
        public JObject Annotations
        {
            get => this.GetPropertyValue<JObject>(keyAnnotations);
            set => this.SetPropertyValue(keyAnnotations, value);
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

        [EntityProperty(keyDimension)]
        public string Dimension
        {
            get => this.GetPropertyValue<string>(keyDimension);
            set => this.SetPropertyValue(keyDimension, value);
        }

        public void OnCreate(string createdBy, DateTime? createdAt = null)
        {
            var now = createdAt ?? DateTime.UtcNow;
            this.CreatedAt = now;
            this.LastModifiedAt = now;
            this.CreatedBy = createdBy;
            this.LastModifiedBy = createdBy;
        }

        public void OnModify(string modifiedBy, DateTime? modifiedAt = null)
        {
            this.LastModifiedAt = modifiedAt ?? DateTime.UtcNow;
            this.LastModifiedBy = modifiedBy;
        }

        public void OnReplace(RuleWrapper existed)
        {
            Ensure.IsNotNull(existed, nameof(existed));
            this.CreatedAt = existed.CreatedAt;
            this.CreatedBy = existed.CreatedBy;
            this.Id = existed.Id;
        }
    }
}