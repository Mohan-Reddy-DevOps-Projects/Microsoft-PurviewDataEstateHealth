// -----------------------------------------------------------------------
// <copyright file="BasicObserverWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [EntityWrapper("Basic", EntityCategory.Observer)]
    public class BasicObserverWrapper : ObserverWrapper
    {
        public const string KeyFavouriteColumnPaths = "favouriteColumnPaths";
        public const string KeyCatalogQualifiedName = "catalogQualifiedName";
        public const string KeyCatalogEntityType = "catalogEntityType";

        private const string keyProjection = "projection";
        private const string keyInputDatasets = "inputDatasets";
        private const string keyRules = "rules";

        public BasicObserverWrapper(JObject jObject) : base(jObject) { }

        private ProjectionWrapper projection;

        [EntityTypeProperty(keyProjection)]
        public ProjectionWrapper Projection
        {
            get
            {
                this.projection ??= this.GetTypePropertyValueAsWrapper<ProjectionWrapper>(keyProjection);
                return this.projection;
            }

            set
            {
                this.SetTypePropertyValueFromWrapper(keyProjection, value);
                this.projection = value;
            }
        }

        private IEnumerable<InputDatasetWrapper> inputDatasets;

        [EntityTypeProperty(keyInputDatasets)]
        [EntityRequiredValidator]
        public IEnumerable<InputDatasetWrapper> InputDatasets
        {
            get
            {
                this.inputDatasets ??= this.GetTypePropertyValueAsWrappers<InputDatasetWrapper>(keyInputDatasets);
                return this.inputDatasets;
            }

            set
            {
                this.SetTypePropertyValueFromWrappers(keyRules, value);
                this.inputDatasets = value;
            }
        }

        private IEnumerable<RuleWrapper> rules;

        [EntityTypeProperty(keyRules)]
        [EntityRequiredValidator]
        public IEnumerable<RuleWrapper> Rules
        {
            get
            {
                this.rules ??= this.GetTypePropertyValueAsWrappers<RuleWrapper>(keyRules);
                return this.rules;
            }

            set
            {
                this.SetTypePropertyValueFromWrappers(keyRules, value);
                this.rules = value;
            }
        }

        private IEnumerable<string> favouriteColumnPaths;

        [EntityTypeProperty(KeyFavouriteColumnPaths)]
        public IEnumerable<string> FavouriteColumnPaths
        {
            get
            {
                this.favouriteColumnPaths ??= this.GetTypePropertyValues<string>(KeyFavouriteColumnPaths);
                return this.favouriteColumnPaths;
            }

            set
            {
                this.SetTypePropertyValue(KeyFavouriteColumnPaths, value);
                this.favouriteColumnPaths = value;
            }
        }

        public RuleWrapper GetRuleById(string ruleId)
        {
            return this.Rules?.FirstOrDefault((e) => String.Equals(e.Id, ruleId, StringComparison.OrdinalIgnoreCase));
        }

        public override BaseEntityWrapper GetEnumeratePayloadWrapper()
        {
            var result = base.GetEnumeratePayloadWrapper();
            var primaryDataSet = this.InputDatasets.FirstOrDefault((item) => item.Primary)?.Dataset;
            if (primaryDataSet != null)
            {
                result.SetPropertyValue("primaryDatasetType", primaryDataSet.Type);
            }
            return result;
        }

        public override void OnCreate(string createdBy, DateTime? createdAt = null)
        {
            base.OnCreate(createdBy, createdAt);
            foreach (var rule in this.Rules)
            {
                rule.OnCreate(createdBy, this.CreatedAt);
            }
        }
    }
}
