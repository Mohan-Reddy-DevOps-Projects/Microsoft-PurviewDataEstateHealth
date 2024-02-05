// -----------------------------------------------------------------------
// <copyright file="CustomTruthRuleWrapper.cs" company="Microsoft Corporation">
//        Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.DataQuality.Rule
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Validators;
    using Newtonsoft.Json.Linq;

    [EntityWrapper(EntityType, EntityCategory.Rule)]
    public class CustomTruthRuleWrapper : CustomRuleWrapper
    {
        public const string EntityType = "CustomTruth";

        private const string keyCondition = "condition";
        private const string keyEmptyCriteria = "emptyCriteria";

        public CustomTruthRuleWrapper(JObject jObject) : base(jObject)
        {
        }

        [EntityTypeProperty(keyCondition)]
        [EntityRequiredValidator]
        public string Condition
        {
            get => this.GetTypePropertyValue<string>(keyCondition);
            set => this.SetTypePropertyValue(keyCondition, value);
        }

        [EntityTypeProperty(keyEmptyCriteria)]
        public string EmptyCriteria
        {
            get => this.GetTypePropertyValue<string>(keyEmptyCriteria);
            set => this.SetTypePropertyValue(keyEmptyCriteria, value);
        }
    }
}
