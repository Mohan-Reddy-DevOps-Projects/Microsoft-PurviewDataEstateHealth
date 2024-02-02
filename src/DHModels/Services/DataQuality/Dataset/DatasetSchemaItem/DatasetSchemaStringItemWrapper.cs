// -----------------------------------------------------------------------
// <copyright file="DatasetSchemaStringItemWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Newtonsoft.Json.Linq;

    [EntityWrapper("String", EntityCategory.DatasetSchemaItem)]
    public class DatasetSchemaStringItemWrapper : DatasetSchemaItemWrapper
    {
        public DatasetSchemaStringItemWrapper(JObject jObject) : base(jObject)
        {
        }
    }
}
