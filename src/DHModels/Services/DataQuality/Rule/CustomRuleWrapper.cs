// -----------------------------------------------------------------------
// <copyright file="CustomRuleWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Newtonsoft.Json.Linq;

    public class CustomRuleWrapper : RuleWrapper
    {
        private const string keyFilterCriteria = "filterCriteria";

        public CustomRuleWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyFilterCriteria)]
        public string FilterCriteria
        {
            get => this.GetTypePropertyValue<string>(keyFilterCriteria);
            set => this.SetTypePropertyValue(keyFilterCriteria, value);
        }
    }
}
