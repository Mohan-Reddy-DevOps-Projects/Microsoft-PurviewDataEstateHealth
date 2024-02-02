// -----------------------------------------------------------------------
// <copyright file="DatasetWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class DatasetWrapper : DynamicEntityWrapper
    {
        private const string keyNativeSchema = "schema";
        private const string keyProjectAs = "projectAs";

        private const string keyBusinessDomain = "businessDomain";
        private const string keyDataProduct = "dataProduct";
        private const string keyDataAsset = "dataAsset";

        private const string keyDatasourceFQN = "datasourceFQN";

        public static DatasetWrapper Create(JObject obj)
        {
            return EntityWrapperHelper.CreateEntityWrapper<DatasetWrapper>(EntityCategory.Dataset, EntityWrapperHelper.GetEntityType(obj), obj);
        }

        public DatasetWrapper(JObject jObject) : base(jObject) { }

        private DatasetSchemaWrapper nativeSchema;

        [EntityProperty(keyNativeSchema)]
        [EntityRequiredValidator]
        public DatasetSchemaWrapper NativeSchema
        {
            get
            {
                this.nativeSchema ??= this.GetPropertyValueAsWrapper<DatasetSchemaWrapper>(keyNativeSchema);
                return this.nativeSchema;
            }

            set
            {
                this.SetPropertyValueFromWrapper(keyNativeSchema, value);
                this.nativeSchema = value;
            }
        }

        [EntityProperty(keyDatasourceFQN)]
        //[EntityRegexValidator("^[^A-Z]+$")] //FQNs have to contain lower-case. TODO: Update once UI changes are done to prod
        [EntityRequiredValidator]
        public string DatasourceFQN
        {
            get => this.GetPropertyValue<string>(keyDatasourceFQN).ToLowerInvariant();
            set => this.SetPropertyValue(keyDatasourceFQN, value);
        }

        private IEnumerable<DatasetProjectAsItemWrapper> projectAs;

        [EntityProperty(keyProjectAs)]
        [EntityRequiredValidator]
        public IEnumerable<DatasetProjectAsItemWrapper> ProjectAs
        {
            get
            {
                this.projectAs ??= this.GetPropertyValueAsWrappers<DatasetProjectAsItemWrapper>(keyProjectAs);
                return this.projectAs;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyProjectAs, value);
                this.projectAs = value;
            }
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

        public virtual string BuildQualifiedPath()
        {
            return string.Empty;
        }
    }
}
