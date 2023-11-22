from DataEstateHealthLibrary.ActionCenter.action_center_transformations import ActionCenterTransformations
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import *
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.ActionCenter.action_center_constants import ActionCenterConstants
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.shared_transformations import SharedTransformations

class ActionCenterAggregation:
    def aggregate_actions(existing_action_df,dataproduct_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association,
                          assetproduct_association_df,dataproduct_action_df,asset_action_df,businessdomain_action_df):

        #here we dont need to check other dataproduct association df's as they are from same source. So one check is enough
        if not dataproduct_df.isEmpty():
            dataproduct_action_df = ActionCenterAggregation.aggregated_data_product_actions(dataproduct_df,dataproduct_contact_association,dataproduct_domain_association)
            
        #here we dont need to check other dataasset association df's as they are from same source. So one check is enough
        if not data_asset_df.isEmpty():
            asset_action_df = ActionCenterAggregation.aggregated_data_asset_actions(data_asset_df,dataasset_contact_association,dataproduct_domain_association,assetproduct_association_df)

        if not businessdomain_df.isEmpty(): 
            businessdomain_action_df = ActionCenterAggregation.aggregated_business_domain_actions(businessdomain_df)
            
        #union_df = [dataproduct_action_df,asset_action_df,businessdomain_action_df]

        merged_action_df = dataproduct_action_df.unionByName(asset_action_df,allowMissingColumns=True)
        merged_action_df = merged_action_df.unionByName(businessdomain_action_df,allowMissingColumns=True)
        
        if existing_action_df.isEmpty():
            return merged_action_df
        else:
            final_merged_action_df = ActionCenterTransformations.generate_final_merged_action_center(merged_action_df, existing_action_df)
            return final_merged_action_df

    def aggregated_business_domain_actions(businessdomain_df):
        businessdomain_action_df = businessdomain_df.select("BusinessDomainId","HasValidOwner","HasDescription","BusinessDomainDisplayName")
        
        #need it for aggregation later        
        businessdomain_action_df = HelperFunction.calculate_default_column_value(businessdomain_action_df,"DataProductId","")
        businessdomain_action_df = HelperFunction.calculate_default_column_value(businessdomain_action_df,"DataAssetId","")
                 
        businessdomain_action_df = ActionCenterTransformations.create_business_domain_actions(businessdomain_action_df)
        businessdomain_action_df = businessdomain_action_df.select("RowId","ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")

        return businessdomain_action_df
    
    def aggregated_data_product_actions(dataproduct_df,dataproduct_contact_association, dataproduct_domain_association):
        dataproduct_action_df = dataproduct_contact_association.join(dataproduct_df,"DataProductId","leftouter")
        dataproduct_action_df = dataproduct_action_df.join(dataproduct_domain_association,"DataProductId","leftouter")
        
        dataproduct_action_df = dataproduct_action_df.select("DataProductId","BusinessDomainId","DataProductDisplayName","HasValidOwner","HasDescription","HasValidTermsOfUse","ContactRole", "ContactId", "ContactDescription",
                                                         "HasValidUseCase","HasAccessEntitlement","HasDataShareAgreementSetOrExempt","HasDataQualityScore")
        #need it for aggregation later
        dataproduct_action_df = HelperFunction.calculate_default_column_value(dataproduct_action_df,"DataAssetId","")
        #we might get some nulls in domain id after join. 
        dataproduct_action_df = HelperFunction.update_null_values(dataproduct_action_df, "BusinessDomainId", "")
        
        dataproduct_action_df = ActionCenterTransformations.create_data_product_actions(dataproduct_action_df)
        dataproduct_action_df = dataproduct_action_df.select("RowId","ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")
        return dataproduct_action_df
    
    def aggregated_data_asset_actions(dataasset_df,dataasset_contact_association,dataproduct_domain_association,assetproduct_association_df):
        asset_action_df = dataasset_contact_association.join(dataasset_df,"DataAssetId","leftouter")
        asset_action_df = asset_action_df.select("DataAssetId","ContactId", "ContactRole","ContactDescription","HasDescription")

        #need it for aggregation later
        if assetproduct_association_df.isEmpty() or dataproduct_domain_association.isEmpty():
            asset_action_df = HelperFunction.calculate_default_column_value(asset_action_df,"DataProductId","")
            asset_action_df = HelperFunction.calculate_default_column_value(asset_action_df,"BusinessDomainId","")
        else:
            assetproductdomain_association_df = SharedTransformations.calculate_assetproductdomain_association(dataproduct_domain_association ,assetproduct_association_df)
            assetproductdomain_association_df = HelperFunction.calculate_default_column_value(assetproductdomain_association_df,"DataProductId","")
            asset_action_df = asset_action_df.join(assetproductdomain_association_df,"DataAssetId","leftouter")
            #we might get some nulls in domain id after join. 
            asset_action_df = HelperFunction.update_null_values(asset_action_df, "BusinessDomainId", "")
            
        asset_action_df = ActionCenterTransformations.create_data_asset_actions(asset_action_df)
        asset_action_df = asset_action_df.select("RowId","ActionId","DisplayName","Description","TargetType","TargetId","TargetName","OwnerContactId","OwnerContactDisplayName","HealthControlState",
                                                         "HealthControlName","HealthControlCategory","ActionStatus","BusinessDomainId","DataProductId","DataAssetId","CreatedAt", "LastRefreshedAt")
        
        return asset_action_df
     
    def aggregated_health_action_contact(aggregated_actions_df):
        health_action_contact_df = ActionCenterTransformations.create_health_action_contact(aggregated_actions_df)
        health_action_contact_df = health_action_contact_df.select("HealthActionId","ContactId", "IsActive", "ContactRole")
        return health_action_contact_df
