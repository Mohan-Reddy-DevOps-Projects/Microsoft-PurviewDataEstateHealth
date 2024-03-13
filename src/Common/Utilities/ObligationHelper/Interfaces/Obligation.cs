// <copyright file="Obligation.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    /// <summary>
    /// Decision for an obligation check
    /// </summary>
    public class Obligation
    {
        /// <summary>
        /// Obligation Type - "Permit" or "NotApplicable"
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("containerType")]
        public string ContainerType { get; set; }

        /// <summary>
        /// List of business domains for the obligation type
        /// If type is Permit, this is the list of business domains the caller has the requested permission to
        /// If type is Permit and list is empty, caller doesn't have the requested permission to any business domain
        /// If type is NotApplicable, this is the list of business domains the caller does not have the requested permission to
        /// If type is NotApplicable and list is empty, caller has requested permission to all business domains
        /// </summary>
        [JsonProperty("collections", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> BusinessDomains { get; set; }

        public static readonly string ContainerTypePrefix = "Purview:";
        public static readonly string PermissionPrefix = "Microsoft.Purview/";
    }

    public class ObligationDictionary : Dictionary<string, Dictionary<string, Obligation>>
    {
        public ObligationDictionary() : base() { }

        public ObligationDictionary(IDictionary<string, Dictionary<string, Obligation>> dictionary) : base(dictionary) { }
    }
}
