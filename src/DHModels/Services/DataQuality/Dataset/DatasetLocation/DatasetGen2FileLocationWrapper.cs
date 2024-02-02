// <copyright file="DatasetGen2FileLocationWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper("AdlsGen2FileLocation", EntityCategory.DatasetLocation)]
    public class DatasetGen2FileLocationWrapper : DatasetLocationWrapper
    {
        private const string keyFileSystem = "fileSystem";
        private const string keyFolderPath = "folderPath";
        private const string keyFileName = "fileName";

        public DatasetGen2FileLocationWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyFileSystem)]
        [EntityRequiredValidator]
        public string FileSystem
        {
            get => this.GetTypePropertyValue<string>(keyFileSystem);
            set => this.SetTypePropertyValue(keyFileSystem, value);
        }

        [EntityTypeProperty(keyFolderPath)]
        public string FolderPath
        {
            get => this.GetTypePropertyValue<string>(keyFolderPath);
            set => this.SetTypePropertyValue(keyFolderPath, value);
        }

        // TODO Check FileName is required for csv, parquet, but not for delta
        [EntityTypeProperty(keyFileName)]
        public string FileName
        {
            get => this.GetTypePropertyValue<string>(keyFileName);
            set => this.SetTypePropertyValue(keyFileName, value);
        }

        public override string BuildQualifiedPath()
        {
            return $"{this.FileSystem.ToLowerInvariant()}/" +
                (!string.IsNullOrWhiteSpace(this.FolderPath) ? $"{this.FolderPath}/" : string.Empty) +
                (string.IsNullOrWhiteSpace(this.FileName) ? string.Empty : this.FileName);
        }
    }
}
