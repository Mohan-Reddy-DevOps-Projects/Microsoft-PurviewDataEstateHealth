// <copyright file="ContactWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

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
        private const string keyDatabaseAdmin = "databaseAdmin";

        public static ContactWrapper Create(JObject jObject)
        {
            return new ContactWrapper(jObject);
        }

        public ContactWrapper(JObject jObject) : base(jObject)
        {
        }

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

        private IEnumerable<ContactItemWrapper> databaseAdmins;

        [EntityProperty(keyDatabaseAdmin)]
        public IEnumerable<ContactItemWrapper> DatabaseAdmins
        {
            get
            {
                this.databaseAdmins ??= this.GetPropertyValueAsWrappers<ContactItemWrapper>(keyDatabaseAdmin);
                return this.databaseAdmins;
            }

            set
            {
                this.SetPropertyValueFromWrappers(keyDatabaseAdmin, value);
                this.databaseAdmins = value;
            }
        }
    }
}
