// <copyright file="DatasetAzureStorageWildcardLocationWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper("AzureStorageWildcardLocation", EntityCategory.DatasetLocation)]
    public class DatasetAzureStorageWildcardLocationWrapper : DatasetLocationWrapper
    {
        private const string keyContainer = "container";
        private const string keyWildcardPath = "wildcardPath";

        public DatasetAzureStorageWildcardLocationWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyContainer)]
        [EntityRequiredValidator]
        public string Container
        {
            get => this.GetTypePropertyValue<string>(keyContainer);
            set => this.SetTypePropertyValue(keyContainer, value);
        }

        [EntityTypeProperty(keyWildcardPath)]
        public string WildcardPath
        {
            get => this.GetTypePropertyValue<string>(keyWildcardPath);
            set => this.SetTypePropertyValue(keyWildcardPath, value);
        }

        public override string BuildQualifiedPath()
        {
            return LocationUtils.BuildQualifiedPathForWildcardLocation(this.Container, this.WildcardPath);
        }
    }
}
