// <copyright file="DatasetAzureStorageFileLocationWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper("AzureStorageFileLocation", EntityCategory.DatasetLocation)]
    public class DatasetAzureStorageFileLocationWrapper : DatasetLocationWrapper
    {
        private const string keyContainer = "container";
        private const string keyFolderPath = "folderPath";
        private const string keyFileName = "fileName";

        public DatasetAzureStorageFileLocationWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyContainer)]
        [EntityRequiredValidator]
        public string Container
        {
            get => this.GetTypePropertyValue<string>(keyContainer);
            set => this.SetTypePropertyValue(keyContainer, value);
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
            return $"{this.Container.ToLowerInvariant()}/" +
                (!string.IsNullOrWhiteSpace(this.FolderPath) ? $"{this.FolderPath}/" : string.Empty) +
                (string.IsNullOrWhiteSpace(this.FileName) ? string.Empty : this.FileName);
        }
    }
}
