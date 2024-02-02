// -----------------------------------------------------------------------
// <copyright file="DatasetProjectAsItemWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset.DatasetProjectAsItem
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Helpers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;
    using System;

    public class DatasetProjectAsItemWrapper : DynamicEntityWrapper
    {
        public const string KeyColumnPath = "columnPath";

        public static DatasetProjectAsItemWrapper Create(JObject obj)
        {
            // As for projectAsItem, type may be absent if it is computed from native schema when validating in resolve API
            string type = null;
            try
            {
                type = EntityWrapperHelper.GetEntityType(obj);
            }
            catch (EntityValidationException)
            {
                // type is not found
                return new DatasetProjectAsItemWrapper(obj);
            }

            return EntityWrapperHelper.CreateEntityWrapper<DatasetProjectAsItemWrapper>(EntityCategory.DatasetProjectAsItem, type, obj);
        }

        public DatasetProjectAsItemWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityProperty(KeyColumnPath)]
        [EntityRequiredValidator]
        public string ColumnPath
        {
            get => this.GetPropertyValue<string>(KeyColumnPath);
            set => this.SetPropertyValue(KeyColumnPath, value);
        }

        public virtual string GetSparkType() => throw new NotImplementedException();
    }
}
