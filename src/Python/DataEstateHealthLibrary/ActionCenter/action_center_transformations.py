import pyspark.sql.functions as f
from pyspark.sql.functions import *
from DataEstateHealthLibrary.ActionCenter.action_center_constants import ActionCenterConstants
from pyspark.sql.functions import *
import datetime
import random
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
import uuid

class ActionCenterTransformations:
    
    def calculate_int_date(action_center_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        int_date_added = action_center_df.withColumn(
            "Date", lit(date_int)
        )

        return int_date_added
    
    def calculate_action_id(action_center_df):
        uuid_udf = f.udf(lambda : str(uuid.uuid4()), StringType())
        action_id_added = action_center_df.withColumn(
            "ActionId", f.expr("uuid()")
            )
        
        return action_id_added
    
    def calculate_created_at(action_center_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        
        created_at_added = action_center_df.withColumn(
            "CreatedAt", lit(date_int)
        )

        return created_at_added
    
    def calculate_last_refreshed_at(action_center_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        
        last_refreshed_at_added = action_center_df.withColumn(
            "LastRefreshedAt", lit(date_int)
        )

        return last_refreshed_at_added

    def create_health_action_contact(action_df):
        action_df = ColumnFunctions.rename_col(action_df,"ActionId", "HealthActionId")
        action_df = ColumnFunctions.rename_col(action_df,"OwnerContactId", "ContactId")
        return action_df

    def create_actions(asset_action_df):
        asset_action_df = ActionCenterTransformations.calculate_action_id(asset_action_df)
        asset_action_df = ActionCenterTransformations.calculate_last_refreshed_at(asset_action_df)
        asset_action_df = ActionCenterTransformations.calculate_created_at(asset_action_df)
        asset_action_df = ActionCenterTransformations.calculate_action_status(asset_action_df)
        asset_action_df = ActionCenterTransformations.calculate_health_control_state(asset_action_df)
        
        asset_action_df = ColumnFunctions.rename_col(asset_action_df,"ContactId", "OwnerContactId")
        asset_action_df = ColumnFunctions.rename_col(asset_action_df,"ContactDescription", "OwnerContactDisplayName")
        
        return asset_action_df
    
    def create_data_asset_actions(asset_action_df):
        
        #HasDescription
        asset_action_df = asset_action_df.filter(col("HasDescription") < 1)
        asset_action_df = ActionCenterTransformations.calculate_action_display_name(asset_action_df,ActionCenterConstants.DESCRIPTION_DISPLAY_NAME_STR)
        asset_action_df = ActionCenterTransformations.calculate_action_description(asset_action_df,ActionCenterConstants.DA_VALID_DESCRIPTION_STR)
        asset_action_df = ActionCenterTransformations.calculate_action_health_control_category(asset_action_df,ActionCenterConstants.GOVERNANCE_STR)
        asset_action_df = ActionCenterTransformations.calculate_action_health_control_name(asset_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        #common transformations
        asset_action_df = ActionCenterTransformations.calculate_target_type(asset_action_df, ActionCenterConstants.DATA_ASSET_STR)
        asset_action_df = ActionCenterTransformations.calculate_target_id(asset_action_df, "DataAssetId")
        asset_action_df = ActionCenterTransformations.calculate_target_name(asset_action_df, "DataAssetId")
        asset_action_df = ActionCenterTransformations.create_actions(asset_action_df)
        return asset_action_df
    
    def create_data_product_actions(dataproduct_action_df):
        
        #HasValidUseCase
        validusecase_action_df = dataproduct_action_df.filter(col("HasValidUseCase") < 1)
        validusecase_action_df = ActionCenterTransformations.calculate_action_display_name(validusecase_action_df,ActionCenterConstants.DP_USECASE_DISPLAY_NAME_STR)
        validusecase_action_df = ActionCenterTransformations.calculate_action_description(validusecase_action_df,ActionCenterConstants.DP_VALID_USECASE_STR)
        validusecase_action_df = ActionCenterTransformations.calculate_action_health_control_category(validusecase_action_df,ActionCenterConstants.GOVERNANCE_STR)
        validusecase_action_df = ActionCenterTransformations.calculate_action_health_control_name(validusecase_action_df,ActionCenterConstants.COMPLETENESS_STR)

        #HasDataShareAgreementSetOrExempt
        datashareagreementsetorexempt_action_df = dataproduct_action_df.filter(col("HasDataShareAgreementSetOrExempt") < 1)
        datashareagreementsetorexempt_action_df = ActionCenterTransformations.calculate_action_display_name(datashareagreementsetorexempt_action_df,ActionCenterConstants.DP_DATASHARESETOREXEMPT_DISPLAY_NAME_STR)
        datashareagreementsetorexempt_action_df = ActionCenterTransformations.calculate_action_description(datashareagreementsetorexempt_action_df,ActionCenterConstants.DP_VALID_DATASHARESETOREXEMPT_STR)
        datashareagreementsetorexempt_action_df = ActionCenterTransformations.calculate_action_health_control_category(datashareagreementsetorexempt_action_df,ActionCenterConstants.GOVERNANCE_STR)
        datashareagreementsetorexempt_action_df = ActionCenterTransformations.calculate_action_health_control_name(datashareagreementsetorexempt_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        #HasDataQualityScore
        dataqualityscore_action_df = dataproduct_action_df.filter(col("HasDataQualityScore") < 1)
        dataqualityscore_action_df = ActionCenterTransformations.calculate_action_display_name(dataqualityscore_action_df,ActionCenterConstants.DP_DATAQUALITY_DISPLAY_NAME_STR)
        dataqualityscore_action_df = ActionCenterTransformations.calculate_action_description(dataqualityscore_action_df,ActionCenterConstants.DP_VALID_DATAQUALITY_STR)
        dataqualityscore_action_df = ActionCenterTransformations.calculate_action_health_control_category(dataqualityscore_action_df,ActionCenterConstants.DATA_QUALITY_STR)
        dataqualityscore_action_df = ActionCenterTransformations.calculate_action_health_control_name(dataqualityscore_action_df,ActionCenterConstants.COMPLETENESS_STR)

        #HasValidTermsofUse
        termsofuse_action_df = dataproduct_action_df.filter(col("HasValidTermsofUse") < 1)
        termsofuse_action_df = ActionCenterTransformations.calculate_action_display_name(termsofuse_action_df,ActionCenterConstants.DP_TERMSOFUSE_DISPLAY_NAME_STR)
        termsofuse_action_df = ActionCenterTransformations.calculate_action_description(termsofuse_action_df,ActionCenterConstants.DP_VALID_TERMSOFUSE_STR)
        termsofuse_action_df = ActionCenterTransformations.calculate_action_health_control_category(termsofuse_action_df,ActionCenterConstants.GOVERNANCE_STR)
        termsofuse_action_df = ActionCenterTransformations.calculate_action_health_control_name(termsofuse_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        #HasDescription
        hasdescription_action_df = dataproduct_action_df.filter(col("HasDescription") < 1)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_display_name(hasdescription_action_df,ActionCenterConstants.DESCRIPTION_DISPLAY_NAME_STR)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_description(hasdescription_action_df,ActionCenterConstants.DP_VALID_DESCRIPTION_STR)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_health_control_category(hasdescription_action_df,ActionCenterConstants.GOVERNANCE_STR)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_health_control_name(hasdescription_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        #HasValidOwner
        validowner_action_df = dataproduct_action_df.filter(col("HasValidOwner") < 1)
        validowner_action_df = ActionCenterTransformations.calculate_action_display_name(validowner_action_df,ActionCenterConstants.OWNER_DISPLAY_NAME_STR)
        validowner_action_df = ActionCenterTransformations.calculate_action_description(validowner_action_df,ActionCenterConstants.DP_VALID_OWNER_STR)
        validowner_action_df = ActionCenterTransformations.calculate_action_health_control_category(validowner_action_df,ActionCenterConstants.GOVERNANCE_STR)
        validowner_action_df = ActionCenterTransformations.calculate_action_health_control_name(validowner_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        #HasAccessEntitlement
        accessentitlement_action_df = dataproduct_action_df.filter(col("HasAccessEntitlement") < 1)
        accessentitlement_action_df = ActionCenterTransformations.calculate_action_display_name(accessentitlement_action_df,ActionCenterConstants.DP_ACCESSENTITLEMENT_DISPLAY_NAME_STR)
        accessentitlement_action_df = ActionCenterTransformations.calculate_action_description(accessentitlement_action_df,ActionCenterConstants.DP_VALID_ACCESSENTITLEMENT_STR)
        accessentitlement_action_df = ActionCenterTransformations.calculate_action_health_control_category(accessentitlement_action_df,ActionCenterConstants.GOVERNANCE_STR)
        accessentitlement_action_df = ActionCenterTransformations.calculate_action_health_control_name(accessentitlement_action_df,ActionCenterConstants.COMPLETENESS_STR)
        
        union_df = [validusecase_action_df,datashareagreementsetorexempt_action_df,dataqualityscore_action_df,termsofuse_action_df,hasdescription_action_df,validowner_action_df,accessentitlement_action_df]
        merged_dataproduct_action_df = reduce(DataFrame.unionAll, union_df)

        #common transformations
        merged_dataproduct_action_df = ActionCenterTransformations.calculate_target_type(merged_dataproduct_action_df, ActionCenterConstants.DATA_PRODUCT_STR)
        merged_dataproduct_action_df = ActionCenterTransformations.calculate_target_id(merged_dataproduct_action_df, "DataProductId")
        merged_dataproduct_action_df = ActionCenterTransformations.calculate_target_name(merged_dataproduct_action_df, "DataProductDisplayName")
        merged_dataproduct_action_df = ActionCenterTransformations.create_actions(merged_dataproduct_action_df)
        return merged_dataproduct_action_df
    
    def create_business_domain_actions(businessdomain_action_df):
        
        # adding default for now as business domain doesn't have contact info
        businessdomain_action_df = ActionCenterTransformations.calculate_default_contact_description(businessdomain_action_df)
        businessdomain_action_df = ActionCenterTransformations.calculate_default_contact_id(businessdomain_action_df)
        
        #HasDescription
        hasdescription_action_df = businessdomain_action_df.filter(col("HasDescription") < 1)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_display_name(hasdescription_action_df,ActionCenterConstants.DESCRIPTION_DISPLAY_NAME_STR)
        hasdescription_action_df = ActionCenterTransformations.calculate_action_description(hasdescription_action_df,ActionCenterConstants.BD_VALID_DESCRIPTION_STR)
        
        #HasValidOwner
        validowner_action_df = businessdomain_action_df.filter(col("HasValidOwner") < 1)
        validowner_action_df = ActionCenterTransformations.calculate_action_display_name(validowner_action_df,ActionCenterConstants.OWNER_DISPLAY_NAME_STR)
        validowner_action_df = ActionCenterTransformations.calculate_action_description(validowner_action_df,ActionCenterConstants.BD_VALID_OWNER_STR)

        union_df = [validowner_action_df,hasdescription_action_df]
        merged_businessdomain_action_df = reduce(DataFrame.unionAll, union_df)
        
        merged_businessdomain_action_df = ActionCenterTransformations.calculate_target_type(merged_businessdomain_action_df, ActionCenterConstants.BUSINESS_DOMAIN_STR)
        merged_businessdomain_action_df = ActionCenterTransformations.calculate_target_id(merged_businessdomain_action_df, "BusinessDomainId")
        merged_businessdomain_action_df = ActionCenterTransformations.calculate_target_name(merged_businessdomain_action_df, "BusinessDomainDisplayName")
        merged_businessdomain_action_df = ActionCenterTransformations.calculate_action_health_control_category(merged_businessdomain_action_df,ActionCenterConstants.GOVERNANCE_STR)
        merged_businessdomain_action_df = ActionCenterTransformations.calculate_action_health_control_name(merged_businessdomain_action_df,ActionCenterConstants.COMPLETENESS_STR)
        merged_businessdomain_action_df = ActionCenterTransformations.create_actions(merged_businessdomain_action_df)
        
        return merged_businessdomain_action_df

    def calculate_target_id(action_df, colName):
        action_target_id_added = action_df.withColumn(
            "TargetId", col(colName)
        )

        return action_target_id_added
    
    def calculate_target_type(action_df, value):
        action_target_type_added = action_df.withColumn(
            "TargetType", lit(value)
        )

        return action_target_type_added

    def calculate_target_name(action_df, colName):
        action_target_name_added = action_df.withColumn(
            "TargetName", col(colName)
        )
 
        return action_target_name_added

    def calculate_action_status(action_df):
        action_display_name_added = action_df.withColumn(
            "ActionStatus", lit(random.choice(ActionCenterConstants.ActionStatuses))
        )

        return action_display_name_added

    def calculate_action_display_name(action_df, value):
        action_display_name_added = action_df.withColumn(
            "DisplayName", lit(value)
        )

        return action_display_name_added
    
    def calculate_action_description(action_df, value):
        action_description_added = action_df.withColumn(
            "Description", lit(value)
        )

        return action_description_added

    def calculate_action_health_control_category(action_df, value):
        action_health_control_category_added = action_df.withColumn(
            "HealthControlCategory", lit(value)
        )

        return action_health_control_category_added
    
    def calculate_action_health_control_name(action_df, value):
        action_health_control_name_added = action_df.withColumn(
            "HealthControlName", lit(value)
        )

        return action_health_control_name_added
    
    def calculate_default_contact_description(action_df):
        default_owner_contact_description_added = action_df.withColumn(
            "ContactDescription", lit("")
        )

        return default_owner_contact_description_added
    
    def calculate_default_contact_id(action_df):
        default_owner_contact_id_added = action_df.withColumn(
            "ContactId", lit(None)
        )

        return default_owner_contact_id_added
    
    def calculate_health_control_state(action_df):
        health_control_state_added = action_df.withColumn(
            "HealthControlState", lit("Active")
        )

        return health_control_state_added
            

