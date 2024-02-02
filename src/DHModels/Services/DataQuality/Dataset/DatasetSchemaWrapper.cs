// -----------------------------------------------------------------------
// <copyright file="DatasetSchemaWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class DatasetSchemaWrapper : BaseEntityWrapper
    {
        private const string keyOrigin = "origin";
        private const string keyItems = "items";

        public DatasetSchemaWrapper(JObject jObject) : base(jObject)
        {
        }

        /// <summary>
        /// Where this schema comm from, like profile, assessment, etc.
        /// </summary>
        [EntityProperty(keyOrigin)]
        public string Origin
        {
            get => this.GetPropertyValue<string>(keyOrigin);
            set => this.SetPropertyValue(keyOrigin, value);
        }

        private IEnumerable<DatasetSchemaItemWrapper> items;

        [EntityProperty(keyItems)]
        [EntityRequiredValidator]
        public IEnumerable<DatasetSchemaItemWrapper> Items
        {
            get
            {
                this.items ??= this.GetPropertyValueAsWrappers<DatasetSchemaItemWrapper>(keyItems);
                return this.items;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyItems, value);
                this.items = value;
            }
        }
    }
}
