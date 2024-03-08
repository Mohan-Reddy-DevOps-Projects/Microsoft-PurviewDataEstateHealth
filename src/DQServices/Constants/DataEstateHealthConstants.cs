// <copyright file="DataEstateHealthConstants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>



namespace Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Join;

public class DataEstateHealthConstants
{
    // Keep the same as definition in DataQuality code
    public const string DEH_DOMAIN_ID = "___deh_business_domain_id___";
    public const string DEH_DATA_SOURCE_ID = "___deh_data_source_id___";
    public const string DEH_KEY_DATA_SOURCE_ENDPOINT = "DataSource.Endpoint";

    public const int SAS_TOKEN_EXPIRATION_HOURS = 24;

    public const string ALWAYS_FAIL_RULE_ID = "AlwaysFail";

    public const string SOURCE_DOMAIN_MODEL_FOLDER_PATH = "DomainModel";
    public const string SOURCE_DP_FOLDER_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProduct";
    public const string SOURCE_DP_STATUS_FOLDER_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductStatus";
    public const string SOURCE_DP_DA_ASSIGNMENT_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductAssetAssignment";
    public const string SOURCE_DP_TERM_ASSIGNMENT_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/GlossaryTermDataProductAssignment";
    public const string SOURCE_DP_BD_ASSIGNMENT_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductBusinessDomainAssignment";
    public const string SOURCE_DP_OWNER_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductOwner";
    public const string SOURCE_ACCESS_POLICY_SET_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/AccessPolicySet";
    public const string SOURCE_ACCESS_POLICY_USE_CASE_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/CustomAccessUseCase";
    public const string SOURCE_DQ_DA_RULE_EXECUTION_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataQualityAssetRuleExecution";
    public const string SOURCE_DA_OWNER_ASSIGNMENT_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataAssetOwnerAssignment";

    // TODO always join all for temporary test
    public static readonly JoinRequirement[] ALWAYS_REQUIRED_JOIN_REQUIREMENTS = [
        JoinRequirement.BusinessDomain,
        JoinRequirement.DataProductStatus,
        JoinRequirement.DataProductOwner
    ];
}
