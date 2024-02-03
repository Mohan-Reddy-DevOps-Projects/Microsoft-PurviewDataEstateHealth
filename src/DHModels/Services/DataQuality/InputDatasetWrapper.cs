// -----------------------------------------------------------------------
// <copyright file="InputDatasetWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Dataset;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    public class InputDatasetWrapper : BaseEntityWrapper
    {
        private const string keyAlias = "alias";
        private const string keyDescription = "description";
        private const string keyPrimary = "primary";
        private const string keyDataset = "dataset";

        public InputDatasetWrapper(JObject jObject) : base(jObject) { }

        [EntityProperty(keyAlias)]
        public string Alias
        {
            get => this.GetPropertyValue<string>(keyAlias);
            set => this.SetPropertyValue(keyAlias, value);
        }

        [EntityProperty(keyDescription)]
        public string Description
        {
            get => this.GetPropertyValue<string>(keyDescription);
            set => this.SetPropertyValue(keyDescription, value);
        }

        [EntityProperty(keyPrimary)]
        [EntityRequiredValidator]
        public bool Primary
        {
            get => this.GetPropertyValue<bool>(keyPrimary);
            set => this.SetPropertyValue(keyPrimary, value);
        }

        private DatasetWrapper dataset;

        [EntityProperty(keyDataset)]
        [EntityRequiredValidator]
        public DatasetWrapper Dataset
        {
            get
            {
                this.dataset ??= this.GetPropertyValueAsWrapper<DatasetWrapper>(keyDataset);
                return this.dataset;
            }

            set
            {
                this.SetPropertyValue(keyDataset, value);
                this.dataset = value;
            }
        }
    }
}
