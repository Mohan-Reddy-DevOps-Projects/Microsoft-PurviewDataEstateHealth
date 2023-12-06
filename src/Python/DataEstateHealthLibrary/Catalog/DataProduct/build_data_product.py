import sys
from pyspark.sql import SparkSession
import DataEstateHealthLibrary
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_transformations import DataProductTransformations
from pyspark.sql.functions import *
from pyspark.sql.types import *
from functools import reduce
from pyspark.sql import DataFrame
import DataEstateHealthLibrary.Catalog.catalog_constants as constants
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_column_functions import DataProductColumnFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_config import DataProductConfig
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Shared.dedup_helper_function import DedupHelperFunction
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_transformations import DataAssetTransformations
from DataEstateHealthLibrary.Catalog.Relationship.relationship_transformations import RelationshipTransformations
from DataEstateHealthLibrary.DataAccess.PolicySet.policy_set_transformations import PolicySetTransformations

class BuildDataProduct:

    def build_data_product_schema(dataproduct_df,dataasset_df, assetproduct_association_df, policyset_df, productquality_df):
        #needed for owner col
        dataproduct_df = CatalogColumnFunctions.add_contacts_schema(dataproduct_df)
        
        dataproduct_df = CatalogColumnFunctions.add_system_data_schema(dataproduct_df)
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdAt", "CreatedAt")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdBy", "CreatedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
    
        dataproduct_df = DataProductTransformations.calculate_is_authoritative_source(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_data_share_agreement_set_or_exempt(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_terms_of_use(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_use_case(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_owner(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_description(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_glossary_term_count(dataproduct_df)
        dataproduct_df = HelperFunction.calculate_last_refreshed_at(dataproduct_df,"LastRefreshedAt")
        
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Id", "DataProductId")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Name", "DataProductDisplayName")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Status", "DataProductStatus")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Type", "DataProductType")
        
        if assetproduct_association_df.isEmpty() or dataasset_df.isEmpty():
          dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df, "ClassificationPassCount", 0)
          dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df, "AssetCount", 0)
        else:
          assetcount_df = RelationshipTransformations.calculate_asset_count(assetproduct_association_df)
          dataproduct_df = dataproduct_df.join(assetcount_df,"DataProductId","leftouter")
          
          dataasset_df = dataasset_df.join(assetproduct_association_df,"DataAssetId","leftouter")
          dataasset_df = dataasset_df.select("DataProductId","HasClassification")
          dataasset_df = DataAssetTransformations.calculate_sum_for_classification_count(dataasset_df)
          dataproduct_df = dataproduct_df.join(dataasset_df,"DataProductId","leftouter")
          
          dataproduct_df = HelperFunction.update_null_values(dataproduct_df, "AssetCount", 0)
          dataproduct_df = HelperFunction.update_null_values(dataproduct_df, "ClassificationPassCount", 0)

        if policyset_df.isEmpty():
            dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"HasAccessEntitlement",False)
        else:
            dataproduct_df = dataproduct_df.join(policyset_df,"DataProductId","leftouter")
            
            dataproduct_df = HelperFunction.update_null_values(dataproduct_df, "HasAccessEntitlement", False)

        if productquality_df.isEmpty():
            dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"HasDataQualityScore",False)
        else:
            productquality_df = productquality_df.select("DataProductId", "QualityScore")
            dataproduct_df = dataproduct_df.join(productquality_df,"DataProductId","leftouter")
            dataproduct_df = DataProductTransformations.calculate_has_data_quality_score(dataproduct_df)
        
        #add timestamp for deduping
        dataproduct_df = CatalogTransformationFunctions.add_timestamp_col(dataproduct_df)
        #remove duplicate rows
        dataproduct_df = dataproduct_df.distinct()
        dataproduct_df = dataproduct_df.select("DataProductId","DataProductDisplayName","DataProductType","HasValidOwner","HasValidUseCase","HasValidTermsOfUse","DataProductStatus","AssetCount","CreatedAt","HasDescription","Timestamp",
                                       "CreatedBy","ModifiedAt","ModifiedBy","LastRefreshedAt","IsAuthoritativeSource","HasDataQualityScore","ClassificationPassCount","HasAccessEntitlement","HasDataShareAgreementSetOrExempt","GlossaryTermCount")
       
        #map by data product id and reduce by timestamp
        dedup_rdd = dataproduct_df.rdd.map(lambda x: (x["DataProductId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])
        
        #convert it to dataframe with sample ration 1 so as to avoid error with null column values
        final_dataproduct_df = dedup_rdd3.toDF(sampleRatio=1.0)
        final_dataproduct_df = final_dataproduct_df.select("DataProductId","DataProductDisplayName","DataProductType","DataProductStatus","HasValidOwner","HasValidUseCase","HasValidTermsOfUse",
                                                           "AssetCount","HasDescription","IsAuthoritativeSource","HasDataQualityScore","ClassificationPassCount","HasAccessEntitlement",
                                                           "HasDataShareAgreementSetOrExempt","GlossaryTermCount","CreatedAt","CreatedBy","ModifiedAt","ModifiedBy","LastRefreshedAt")
        
        return final_dataproduct_df

    def build_data_product_contact_association(dataproduct_df):

        dataproduct_df = CatalogColumnFunctions.add_contacts_schema(dataproduct_df)
        dataproduct_owner_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "owner", "Owner", "Contacts")

        dataproduct_expert_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "expert", "Expert", "Contacts")
    
        dataproduct_database_admin_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "databaseAdmin", "DatabaseAdmin", "Contacts")
    
    
        union_df = [dataproduct_owner_df,dataproduct_expert_df,dataproduct_database_admin_df]
        merged_df = reduce(DataFrame.unionAll, union_df)
    
        merged_df = ColumnFunctions.rename_col(merged_df, "Id", "DataProductId")
        data_product_contact_association_df = merged_df.select("DataProductId", "ContactRole", "ContactId", "ContactDescription")
        
        #remove duplicate rows
        data_product_contact_association_df = data_product_contact_association_df.distinct()
        #drop any row which has a null value in listed columns
        data_product_contact_association_df = data_product_contact_association_df.na.drop(subset=["DataProductId","ContactId","ContactRole"])
        return data_product_contact_association_df

    def build_data_product_business_domain_association(dataproduct_df):
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Domain", "BusinessDomainId")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Id", "DataProductId")
        
        dataproduct_df = DataProductTransformations.calculate_is_primary_dataproduct(dataproduct_df)
        
        data_product_business_domain_association_df = dataproduct_df.select("DataProductId", "BusinessDomainId", "IsPrimaryDataProduct")
        #remove duplicate rows
        data_product_business_domain_association_df = data_product_business_domain_association_df.distinct()
        #drop any row which has a null value
        data_product_business_domain_association_df = data_product_business_domain_association_df.na.drop()
        return data_product_business_domain_association_df
    
    def handle_dataproduct_deletes(dataproduct_df, deleted_dataproduct_df):
        if not deleted_dataproduct_df.isEmpty():
            deleted_dataproduct_df = deleted_dataproduct_df.select("Id");
            dataproduct_df = dataproduct_df.join(deleted_dataproduct_df, ["Id"], "leftanti")
        return dataproduct_df
    
    def update_boolean_to_int(dataproduct_df):
        if not dataproduct_df.isEmpty():
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasDataShareAgreementSetOrExempt")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasDataQualityScore")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"IsAuthoritativeSource")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasAccessEntitlement")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasDescription")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasValidOwner")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasValidTermsOfUse")
            dataproduct_df = HelperFunction.update_col_to_int(dataproduct_df,"HasValidUseCase")
        return dataproduct_df
