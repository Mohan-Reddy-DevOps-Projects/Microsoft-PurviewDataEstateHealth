// -----------------------------------------------------------------------
// <copyright file="DatasetSchemaItemWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetSchemaItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    public class DatasetSchemaItemWrapper : DynamicEntityWrapper
    {
        public const string KeyName = "name";

        [EntityProperty(keyType)]
        public new string Type => this.GetPropertyValue<string>(keyType);

        public static DatasetSchemaItemWrapper Create(JObject obj)
        {
            string type = null;
            try
            {
                type = EntityWrapperHelper.GetEntityType(obj);
            }
            catch (EntityValidationException)
            {
                // type is not found
                return new DatasetSchemaItemWrapper(obj);
            }

            return EntityWrapperHelper.CreateEntityWrapper<DatasetSchemaItemWrapper>(EntityCategory.DatasetSchemaItem, EntityWrapperHelper.GetEntityType(obj), obj);
        }

        public DatasetSchemaItemWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityProperty(KeyName)]
        [EntityRequiredValidator]
        public string Name
        {
            get => this.GetPropertyValue<string>(KeyName);
            set => this.SetPropertyValue(KeyName, value);
        }
    }
}
