from DataEstateHealthLibrary.ActionCenter.action_center_transformations import ActionCenterTransformations
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import *
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.ActionCenter.action_center_constants import ActionCenterConstants
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations

class ActionCenterAggregation:
    def aggregate_actions(dataproduct_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association):
        dataproduct_action_df = ActionCenterAggregation.aggregated_data_product_actions(dataproduct_df,dataproduct_contact_association,dataproduct_domain_association)
        asset_action_df = ActionCenterAggregation.aggregated_data_asset_actions(data_asset_df,dataasset_contact_association)
        businessdomain_action_df = ActionCenterAggregation.aggregated_business_domain_actions(businessdomain_df)
        union_df = [dataproduct_action_df,asset_action_df,businessdomain_action_df]
        merged_action_df = reduce(DataFrame.union, union_df)
        return merged_action_df

    def aggregated_business_domain_actions(businessdomain_df):
        businessdomain_df = businessdomain_df.select("BusinessDomainId","HasValidOwner","HasDescription","BusinessDomainDisplayName")
        #need it for aggregation later
        businessdomain_action_df = businessdomain_df.withColumn("DataProductId", lit("00000000-0000-0000-0000-000000000000"))
        businessdomain_action_df = businessdomain_action_df.withColumn("DataAssetId", lit("00000000-0000-0000-0000-000000000000"))
        
        businessdomain_action_df = ActionCenterTransformations.create_business_domain_actions(businessdomain_action_df)
        businessdomain_action_df = businessdomain_action_df.select("ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")

        return businessdomain_action_df
    
    def aggregated_data_product_actions(dataproduct_df,dataproduct_contact_association, dataproduct_domain_association):
        dataproduct_action_df = dataproduct_contact_association.join(dataproduct_df,"DataProductId","leftouter")
        dataproduct_action_df = dataproduct_domain_association.join(dataproduct_action_df,"DataProductId","leftouter")
        
        dataproduct_action_df = dataproduct_action_df.select("DataProductId","BusinessDomainId","DataProductDisplayName","HasValidOwner","HasDescription","HasValidTermsofUse","ContactRole", "ContactId", "ContactDescription",
                                                         "HasValidUseCase","HasAccessEntitlement","HasDataShareAgreementSetOrExempt","HasDataQualityScore")
        #need it for aggregation later
        dataproduct_action_df = dataproduct_action_df.withColumn("DataAssetId", lit("00000000-0000-0000-0000-000000000000"))
        
        dataproduct_action_df = ActionCenterTransformations.create_data_product_actions(dataproduct_action_df)
        dataproduct_action_df = dataproduct_action_df.select("ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")
        return dataproduct_action_df
    
    def aggregated_data_asset_actions(dataasset_df,dataasset_contact_association):
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "DataMapAssetId", "DataAssetId")
        asset_action_df = dataasset_contact_association.join(dataasset_df,"DataAssetId","leftouter")
        asset_action_df = asset_action_df.select("DataAssetId","BusinessDomainId","ContactId", "ContactRole","ContactDescription","HasDescription")
        asset_action_df = ActionCenterTransformations.create_data_asset_actions(asset_action_df)

        #need it for aggregation later
        asset_action_df = asset_action_df.withColumn("DataProductId", lit("00000000-0000-0000-0000-000000000000"))
        
        asset_action_df = asset_action_df.select("ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")
        
        return asset_action_df
     
    def aggregated_health_action_contact(aggregated_actions_df):
        health_action_contact_df = ActionCenterTransformations.create_health_action_contact(aggregated_actions_df)
        health_action_contact_df = health_action_contact_df.select("HealthActionId","ContactId", "IsActive", "ContactRole")
        return health_action_contact_df
