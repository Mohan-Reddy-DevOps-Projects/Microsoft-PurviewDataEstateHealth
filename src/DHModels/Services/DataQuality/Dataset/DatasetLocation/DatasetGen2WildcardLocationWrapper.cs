// <copyright file="DatasetGen2WildcardLocationWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper("AdlsGen2WildcardLocation", EntityCategory.DatasetLocation)]
    public class DatasetGen2WildcardLocationWrapper : DatasetLocationWrapper
    {
        private const string keyFileSystem = "fileSystem";
        private const string keyWildcardPath = "wildcardPath";

        public DatasetGen2WildcardLocationWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyFileSystem)]
        [EntityRequiredValidator]
        public string FileSystem
        {
            get => this.GetTypePropertyValue<string>(keyFileSystem);
            set => this.SetTypePropertyValue(keyFileSystem, value);
        }

        [EntityTypeProperty(keyWildcardPath)]
        public string WildcardPath
        {
            get => this.GetTypePropertyValue<string>(keyWildcardPath);
            set => this.SetTypePropertyValue(keyWildcardPath, value);
        }

        public override string BuildQualifiedPath()
        {
            return LocationUtils.BuildQualifiedPathForWildcardLocation(this.FileSystem, this.WildcardPath);
        }
    }
}
