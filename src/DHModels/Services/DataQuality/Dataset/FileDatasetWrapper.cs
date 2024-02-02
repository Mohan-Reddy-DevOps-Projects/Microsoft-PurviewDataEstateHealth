// <copyright file="FileDatasetWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Linq;

    public class FileDatasetWrapper : DatasetWrapper
    {
        private const string keyLocation = "location";
        private const string KeyCompressionCodec = "compressionCodec";
        private const string KeyRootPartition = "partitionRootPath";

        public FileDatasetWrapper(JObject jObject) : base(jObject)
        {
        }

        private IEnumerable<DatasetLocationWrapper> location;

        [EntityTypeProperty(keyLocation)]
        [EntityArrayValidator(MaxLength = 1, MinLength = 1)]
        [EntityRequiredValidator]
        public IEnumerable<DatasetLocationWrapper> Location
        {
            get
            {
                this.location ??= this.GetTypePropertyValueAsWrappers<DatasetLocationWrapper>(keyLocation);
                return this.location;
            }

            set
            {
                this.SetTypePropertyValueFromWrappers(keyLocation, value);
                this.location = value;
            }
        }

        [EntityTypeProperty(KeyCompressionCodec)]
        public string CompressionCodec
        {
            get => this.GetTypePropertyValue<string>(KeyCompressionCodec);
            set => this.SetTypePropertyValue(KeyCompressionCodec, value);
        }

        [EntityTypeProperty(KeyRootPartition)]
        public string RootPartition
        {
            get => this.GetTypePropertyValue<string>(KeyRootPartition);
            set => this.SetTypePropertyValue(KeyRootPartition, value);
        }

        public override string BuildQualifiedPath()
        {
            // if the location count > 1, we only capture the first one as the index
            // this is used to query the list of datasets
            return this.Location.Any() ? this.Location.First().BuildQualifiedPath() : string.Empty;
        }
    }
}
