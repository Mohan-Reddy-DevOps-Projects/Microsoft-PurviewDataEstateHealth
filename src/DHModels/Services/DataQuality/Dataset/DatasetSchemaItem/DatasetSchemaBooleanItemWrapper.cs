// -----------------------------------------------------------------------
// <copyright file="DatasetSchemaBooleanItemWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------



namespace Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetSchemaItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Newtonsoft.Json.Linq;

    [EntityWrapper(EntityType, EntityCategory.DatasetSchemaItem)]
    public class DatasetSchemaBooleanItemWrapper : DatasetSchemaItemWrapper
    {
        public const string EntityType = "Boolean";

        public DatasetSchemaBooleanItemWrapper(JObject jObject) : base(jObject)
        {
        }
    }
}
