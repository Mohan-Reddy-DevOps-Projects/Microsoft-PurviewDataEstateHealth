// <copyright file="DataEstateHealthConstants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>



namespace Microsoft.Purview.DataEstateHealth.DHModels.Constants;
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
    public const string SOURCE_DP_BD_ASSIGNMENT_PATH = SOURCE_DOMAIN_MODEL_FOLDER_PATH + "/DataProductBusinessDomainAssignment";
}
