// <copyright file="DeltaDatasetWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Newtonsoft.Json.Linq;

    [EntityWrapper(entityType, EntityCategory.Dataset)]
    public class DeltaDatasetWrapper : FileDatasetWrapper
    {
        public const string entityType = "Delta";

        public DeltaDatasetWrapper(JObject jObject) : base(jObject)
        {
        }
    }
}
