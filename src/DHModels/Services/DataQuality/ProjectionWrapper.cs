// -----------------------------------------------------------------------
// <copyright file="ProjectionWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    public enum ProjectionType
    {
        Sql
    }

    public class ProjectionWrapper : BaseEntityWrapper
    {
        private const string keyType = "type";
        private const string keyScript = "script";

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
    }
}
