namespace Microsoft.Purview.DataEstateHealth.DHModels.Constants
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
    using System.Collections.Generic;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public static class DHAssessmentRuleConstants
    {
        public delegate DHAssessmentRuleActionPropertiesWrapper RuleActionPropsDelegate(string operatorValue = "");

        public static RuleActionPropsDelegate DataProductRelatedDataAssetsWithClassificationCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper 
        {
            Name = "The count of related data assets with classification for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of related data assets with classification for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product classification count of related data assets should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };

        public static RuleActionPropsDelegate DataProductAllRelatedAssetsHaveOwner = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} owners for all related assets on data product.",
                Reason = (operatorValue.ToLower() == "istrue") ? "All related assets of data product do not have an owner": "All related assets of data product have an owner",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "All related assets of data product should have an owner" : "All related assets of data product have an owner",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductAllRelatedAssetsHaveDQScore = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} data quality scores for all related assets on data product.",
                Reason = (operatorValue.ToLower() == "istrue") ? "All related assets of data product dont have a data quality scores." : "All related assets of data product have a data quality scores.",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "All related assets of Data product should have a data quality scores." : "All related assets of Data product have a data quality scores.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductBusinessUseLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The business use length of the data product is {oppositeOperator} {operand}.",
            Reason = "The business use length of the data product is {oppositeOperator} {operand}.",
            Recommendation = "The business use length of the data product should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductDescriptionLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The description length of the data product is {oppositeOperator} {operand}.",
            Reason = "The description length of the data product is {oppositeOperator} {operand}.",
            Recommendation = "The description length of the data product should be {operator} {operand}",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductEndorsed = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} endorsement for Data Product.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data product is not endorsed" : "Data product is endorsed",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Data product should be endorsed" : "Data product is endorsed",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductHasDataAccessPolicy = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} access policy on data products.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data product does not have data access policies" : "Data product have data access policies",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Access policies should be applied for the data products." : "access policies are applied for the data products.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductHasDataUsagePurpose = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} Data Usage Purpose on Data Product.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data product does not have data usage purpose." : "Data product does have data usage purpose.",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Data product should have data usage purpose." : "Data product have data usage purpose.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductHasDQScore = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} data quality scores on data products.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data product does not have data quality score." : "Data product have data quality score.",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Data product should have data quality score." : "Data product have data quality score.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductOwnerCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of owners for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of owners for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product owner count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductRelatedDataAssetsCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of related data assets for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of related data assets for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product owner count should be {operator} {operand}",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductRelatedObjectivesCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of related objectives for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of related objectives for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product related data assets count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductRelatedTermsCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of related published glossary terms for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of related published glossary terms for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product related published glossary terms count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductStatus = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The status of the data product is {oppositeOperator} {operand}.",
            Reason = "The status of the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Status of the data propduct should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductAllRelatedTermsMinimalDescriptionLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The minimal description length of all related terms for the data product is {oppositeOperator} {operand}",
            Reason = "The minimal description length of all related terms for the data product is {oppositeOperator} {operand}",
            Recommendation = "The Data product all related published terms description minimal length should be {operator} {operand}",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductDomainDescriptionLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "Business domain description length of data product is {oppositeOperator} {operand}.",
            Reason = "Business domain description length of data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product domain description length should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductDomainHasOwner = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} owners on business domains.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Business domains of data products do not have owners" : "Business domains of data products have owners",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Owners should be assigned for the business domains of the data product." : "Owners are assigned for the business domains of the data product.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductTermsOfUseCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of terms of use for the data product is {oppositeOperator} {operand}.",
            Reason = "The count of terms of use for the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product published terms of use count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductRelatedAssetsHaveDQScore = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} data quality scores on data assets related to Data Product.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data assets of data products do not have data quality scores." : "Data assets of data products have data quality scores.",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Data quality rules should be assigned for the data assets that are related to the data product." : "data quality rules are assinged for the data assets that are related to the data product.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate DataProductRelatedAssetsOwnerCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of owners for related assets of the data product is {oppositeOperator} {operand}.",
            Reason = "The count of owners for related assets of the data product is {oppositeOperator} {operand}.",
            Recommendation = "The Data product related data assets owner count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate DataProductRelatedTermsDescriptionLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "Published glossary terms' description length is {oppositeOperator} {operand}.",
            Reason = "Published glossary terms' description length is {oppositeOperator} {operand}.",
            Recommendation = "The Data product related published terms description length should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate CDEAllRelatedAssetsHaveClassification = (string operatorValue = "") =>
        {
            return new DHAssessmentRuleActionPropertiesWrapper
            {
                Name = "{oppositeOperator} classification on data assets related to critical data entities.",
                Reason = (operatorValue.ToLower() == "istrue") ? "Data assets dont have classification that are part of critical data entities." : "Data assets have classification that are part of critical data entities.",
                Recommendation = (operatorValue.ToLower() == "istrue") ? "Classifications should be applied for the data assets related to the critical data entities." : "Classifications are applied for the data assets related to the critical data entities.",
                Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
            };
        };
        public static RuleActionPropsDelegate CDEDescriptionLength = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The description length of the Critical data entities is {oppositeOperator} {operand}.",
            Reason = "The description length of the Critical data entities is {oppositeOperator} {operand}.",
            Recommendation = "The Critical data entities description length should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate CDEOwnerCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The number of owners for the Critical data entities is {oppositeOperator} {operand}",
            Reason = "The number of owners for the Critical data entities is {oppositeOperator} {operand}.",
            Recommendation = "The Critical data entities owner count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate CDERelatedDataAssetsCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The number of related data assets for the Critical data entities is {oppositeOperator} {operand}",
            Reason = "The number of related data assets for the Critical data entities is {oppositeOperator} {operand}.",
            Recommendation = "The Critical data entities related data assets count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate CDERelatedTermsCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The number of related terms for the Critical data entities is {oppositeOperator} {operand}",
            Reason = "The number of related terms for the Critical data entities is {oppositeOperator} {operand}.",
            Recommendation = "The Critical data entities related terms count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };
        public static RuleActionPropsDelegate BusinessDomainCriticalDataElementCount = (string operatorValue = "") => new DHAssessmentRuleActionPropertiesWrapper
        {
            Name = "The count of Critical data entities in the business domain is {oppositeOperator} {operand}.",
            Reason = "The count of Critical data entities in the business domain is {oppositeOperator} {operand}.",
            Recommendation = "The bussiness domain Critical data entities count should be {operator} {operand}.",
            Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
        };

        public static Dictionary<string, string> OppositeOperators = new Dictionary<string, string>
        {
            { "Equal", "not equal to" },
            { "NotEqual", "equal to" },
            { "LessThan", "greater than or equal to" },
            { "GreaterThan", "less than or equal to" },
            { "LessThanOrEqual", "greater than" },
            { "GreaterThanOrEqual", "less than" },
            { "IsNullOrEmpty", "not Null or Empty" },
            { "IsNotNullOrEmpty", "Null or Empty" },
            { "IsTrue", "Missing" },
            { "IsFalse", "Assigned" }
        };

        public static Dictionary<string, string> Operators = new Dictionary<string, string>
        {
            { "LessThanOrEqual", "less than or equal to" },
            { "GreaterThanOrEqual", "greater than or equal to"},
            { "NotEqual", "not equal to"},
            { "Equal", "equal to"},
            { "LessThan", "less than"},
            { "GreaterThan", "greater than"},
            { "IsNullOrEmpty", "null or empty"},
            { "IsNotNullOrEmpty", "not null or empty"},
            { "IsTrue", "true"},
            { "IsFalse", "false"}
        };

        public static Dictionary<string, RuleActionPropsDelegate> CheckpointDescription = new Dictionary<string, RuleActionPropsDelegate>
        {
            { "DataProductRelatedDataAssetsWithClassificationCount", DataProductRelatedDataAssetsWithClassificationCount },
            { "DataProductAllRelatedAssetsHaveOwner",  DataProductAllRelatedAssetsHaveOwner},
            { "DataProductAllRelatedAssetsHaveDQScore", DataProductAllRelatedAssetsHaveDQScore },
            { "DataProductBusinessUseLength",DataProductBusinessUseLength  },
            { "DataProductDescriptionLength",  DataProductDescriptionLength},
            { "DataProductEndorsed", DataProductEndorsed },
            { "DataProductHasDataAccessPolicy",DataProductHasDataAccessPolicy  },
            { "DataProductHasDataUsagePurpose",DataProductHasDataUsagePurpose  },
            { "DataProductHasDQScore",DataProductHasDQScore  },
            { "DataProductOwnerCount",DataProductOwnerCount  },
            { "DataProductRelatedDataAssetsCount",DataProductRelatedDataAssetsCount  },
            { "DataProductRelatedObjectivesCount",DataProductRelatedObjectivesCount  },
            { "DataProductRelatedTermsCount",DataProductRelatedTermsCount  },
            { "DataProductStatus",DataProductStatus  },
            { "DataProductAllRelatedTermsMinimalDescriptionLength",DataProductAllRelatedTermsMinimalDescriptionLength  },
            { "DataProductDomainDescriptionLength",DataProductDomainDescriptionLength  },
            { "DataProductDomainHasOwner",DataProductDomainHasOwner  },
            { "DataProductTermsOfUseCount",DataProductTermsOfUseCount  },
            { "DataProductRelatedAssetsHaveDQScore",DataProductRelatedAssetsHaveDQScore  },
            { "DataProductRelatedAssetsOwnerCount",DataProductRelatedAssetsOwnerCount  },
            { "DataProductRelatedTermsDescriptionLength",DataProductRelatedTermsDescriptionLength  },
            { "CDEAllRelatedAssetsHaveClassification",CDEAllRelatedAssetsHaveClassification  },
            { "CDEDescriptionLength",CDEDescriptionLength  },
            { "CDEOwnerCount",CDEOwnerCount  },
            { "CDERelatedDataAssetsCount",CDERelatedDataAssetsCount  },
            { "CDERelatedTermsCount",CDERelatedTermsCount  },
            { "BusinessDomainCriticalDataElementCount",BusinessDomainCriticalDataElementCount  }
        };

        public static DHAssessmentRuleActionPropertiesWrapper GetValueOrReturnKey(this Dictionary<string, RuleActionPropsDelegate> keyValues, string key, string operatorValue = "")
        {
            if (keyValues.TryGetValue(key, out RuleActionPropsDelegate? value))
            {
                return value(operatorValue);
            }
            else
            {
                return new DHAssessmentRuleActionPropertiesWrapper
                {
                    Name = key,
                    Reason = key,
                    Recommendation = key,
                    Severity = Services.DataHealthAction.DataHealthActionSeverity.Medium
                };
            }
                
        }
        public static string GetValueOrReturnKey(this Dictionary<string, string> keyValues, string key)
        {
            keyValues.TryGetValue(key, out string? value);
            return string.IsNullOrWhiteSpace(value) ? key : value;
        }
    }
}
