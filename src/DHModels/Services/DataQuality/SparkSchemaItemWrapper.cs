// <copyright file=SparkSchemaItemWrapper.cs company="Microsoft Corporation"> 
// Copyright (c) Microsoft Corporation. All rights reserved. 
// </copyright>

namespace Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    public class SparkSchemaItemWrapper : BaseEntityWrapper
    {
        public const string KeyColumnPath = "columnPath";
        public const string KeyName = "name";
        public const string KeyType = "type";

        public SparkSchemaItemWrapper(JObject obj) : base(obj)
        {
        }

        [EntityProperty(KeyColumnPath)]
        [EntityRequiredValidator]
        public string ColumnPath
        {
            get => this.GetPropertyValue<string>(KeyColumnPath);
            set => this.SetPropertyValue(KeyColumnPath, value);
        }

        [EntityProperty(KeyName)]
        [EntityRequiredValidator]
        public string Name
        {
            get => this.GetPropertyValue<string>(KeyName);
            set => this.SetPropertyValue(KeyName, value);
        }

        [EntityProperty(KeyType)]
        [EntityRequiredValidator]
        public string Type
        {
            get => this.GetPropertyValue<string>(KeyType);
            set => this.SetPropertyValue(KeyType, value);
        }
    }
}
