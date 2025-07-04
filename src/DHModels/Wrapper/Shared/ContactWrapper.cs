﻿// <copyright file="ContactWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Shared
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Base;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class ContactWrapper : BaseEntityWrapper
    {
        // TODO: Add other contact types and custom contact types
        public const string keyOwner = "owner";
        private const string keyExpert = "expert";

        public static ContactWrapper Create(JObject jObject)
        {
            return new ContactWrapper(jObject);
        }

        public ContactWrapper(JObject jObject) : base(jObject)
        {
        }

        public ContactWrapper() : this(new JObject()) { }

        private IEnumerable<ContactItemWrapper> owners;

        [EntityProperty(keyOwner)]
        public IEnumerable<ContactItemWrapper> Owners
        {
            get
            {
                this.owners ??= this.GetPropertyValueAsWrappers<ContactItemWrapper>(keyOwner);
                return this.owners;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyOwner, value);
                this.owners = value;
            }
        }

        private IEnumerable<ContactItemWrapper> experts;

        [EntityProperty(keyExpert)]
        public IEnumerable<ContactItemWrapper> Experts
        {
            get
            {
                this.experts ??= this.GetPropertyValueAsWrappers<ContactItemWrapper>(keyExpert);
                return this.experts;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyExpert, value);
                this.experts = value;
            }
        }
    }
}
