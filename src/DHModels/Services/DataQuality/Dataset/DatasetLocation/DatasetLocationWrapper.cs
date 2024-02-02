// <copyright file="DatasetLocationWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetLocation
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Newtonsoft.Json.Linq;

    public class DatasetLocationWrapper : DynamicEntityWrapper
    {
        public DatasetLocationWrapper(JObject jObject) : base(jObject)
        {
        }

        public static DatasetLocationWrapper Create(JObject obj)
        {
            return EntityWrapperHelper.CreateEntityWrapper<DatasetLocationWrapper>(EntityCategory.DatasetLocation, EntityWrapperHelper.GetEntityType(obj), obj);
        }

        public virtual string BuildQualifiedPath()
        {
            return string.Empty;
        }
    }
}
