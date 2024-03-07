// -----------------------------------------------------------------------
// <copyright file="DatasetSchemaNumberItemWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataQuality.Models.Service.Dataset.DatasetSchemaItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper(EntityType, EntityCategory.DatasetSchemaItem)]
    public class DatasetSchemaNumberItemWrapper : DatasetSchemaItemWrapper
    {
        private const string keyIntegral = "integral";
        private const string keyPrecision = "precision";
        private const string keyScale = "scale";

        public const string EntityType = "Number";

        public DatasetSchemaNumberItemWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyIntegral)]
        [EntityRequiredValidator]
        public bool Integral
        {
            get => this.GetTypePropertyValue<bool>(keyIntegral);
            set => this.SetTypePropertyValue(keyIntegral, value);
        }

        [EntityTypeProperty(keyPrecision)]
        public int? Precision
        {
            get => this.GetTypePropertyValue<int?>(keyPrecision);
            set => this.SetTypePropertyValue(keyPrecision, value);
        }

        [EntityTypeProperty(keyScale)]
        public int? Scale
        {
            get => this.GetTypePropertyValue<int?>(keyScale);
            set => this.SetTypePropertyValue(keyScale, value);
        }
    }
}
