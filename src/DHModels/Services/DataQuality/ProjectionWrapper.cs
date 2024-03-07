// -----------------------------------------------------------------------
// <copyright file="ProjectionWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetProjectAsItem;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public enum ProjectionType
    {
        sql
    }

    public class ProjectionWrapper : BaseEntityWrapper
    {
        private const string keyType = "type";
        private const string keyScript = "script";
        private const string keyNativeSchema = "schema";

        public ProjectionWrapper(JObject jObject) : base(jObject) { }

        [EntityProperty(keyType)]
        [EntityRequiredValidator]
        public ProjectionType Type
        {
            get => this.GetPropertyValue<ProjectionType>(keyType);
            set => this.SetPropertyValue(keyType, value.ToString());
        }

        [EntityProperty(keyScript)]
        public string Script
        {
            get => this.GetPropertyValue<string>(keyScript);
            set => this.SetPropertyValue(keyScript, value);
        }


        private IEnumerable<SparkSchemaItemWrapper> nativeSchema;

        [EntityProperty(keyNativeSchema)]
        [EntityRequiredValidator]
        public IEnumerable<SparkSchemaItemWrapper> NativeSchema
        {
            get
            {
                this.nativeSchema ??= this.GetPropertyValueAsWrappers<SparkSchemaItemWrapper>(keyNativeSchema);
                return this.nativeSchema;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyNativeSchema, value);
                this.nativeSchema = value;
            }
        }
    }
}
