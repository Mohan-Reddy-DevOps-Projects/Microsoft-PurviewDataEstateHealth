// <copyright file="ContactItemWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    public class ContactItemWrapper : BaseEntityWrapper
    {
        public const string keyId = "id";
        private const string keyDescription = "description";

        public static ContactItemWrapper Create(JObject jObject)
        {
            return new ContactItemWrapper(jObject);
        }

        public ContactItemWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityRequiredValidator]
        [EntityProperty(keyId)]
        public string Id
        {
            get => this.GetPropertyValue<string>(keyId);
            set => this.SetPropertyValue(keyId, value);
        }

        [EntityProperty(keyDescription)]
        public string Description
        {
            get => this.GetPropertyValue<string>(keyDescription);
            set => this.SetPropertyValue(keyDescription, value);
        }
    }
}
