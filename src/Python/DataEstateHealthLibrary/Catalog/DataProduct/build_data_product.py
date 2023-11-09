import sys
from pyspark.sql import SparkSession
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

class BuildDataProduct:

    def build_data_product_schema(dataproduct_df):
        #needed for owner col
        dataproduct_df = CatalogColumnFunctions.add_contacts_schema(dataproduct_df)
        
        dataproduct_df = CatalogColumnFunctions.add_system_data_schema(dataproduct_df)
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdAt", "CreatedAt")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdBy", "CreatedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
    
        dataproduct_df = DataProductColumnFunctions.add_additional_properties_schema(dataproduct_df)
        dataproduct_df = DataProductColumnFunctions.add_asset_count(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_is_authoritative_source(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_classification_pass_count(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_access_entitlement(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_data_share_agreement_set_or_exempt(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_terms_of_use(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_use_case(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_valid_owner(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_is_authoritative_source(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_not_null_description(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_is_authoritative_source(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_glossary_term_count(dataproduct_df)
        dataproduct_df = DataProductTransformations.calculate_has_data_quality_score(dataproduct_df)
        dataproduct_df = HelperFunction.calculate_last_refreshed_at(dataproduct_df)
        
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Id", "DataProductId")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Name", "DataProductDisplayName")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Status", "DataPoductStatus")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Type", "DataProductType")
        
        #add timestamp for deduping
        dataproduct_df = CatalogTransformationFunctions.add_timestamp_col(dataproduct_df)
        #remove duplicate rows
        dataproduct_df = dataproduct_df.distinct()
        dataproduct_df = dataproduct_df.select("DataProductId","DataProductDisplayName","DataProductType","HasValidOwner","HasValidUseCase","HasValidTermsofUse","DataPoductStatus","AssetCount","CreatedAt","HasNotNullDescription","Timestamp",
                                       "CreatedBy","ModifiedAt","ModifiedBy","LastRefreshedAt","IsAuthoritativeSource","HasDataQualityScore","ClassificationPassCount","HasAccessEntitlement","HasDataShareAgreementSetOrExempt","GlossaryTermCount")
       
        #map by data product id and reduce by timestamp
        dedup_rdd = dataproduct_df.rdd.map(lambda x: (x["DataProductId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])
        
        #convert it to dataframe with sample ration 1 so as to avoid error with null column values
        final_dataproduct_df = dedup_rdd3.toDF(sampleRatio=1.0)
        final_dataproduct_df = final_dataproduct_df.select("DataProductId","DataProductDisplayName","DataProductType","HasValidOwner","HasValidUseCase","HasValidTermsofUse","DataPoductStatus","AssetCount","CreatedAt","HasNotNullDescription",
                                       "CreatedBy","ModifiedAt","ModifiedBy","LastRefreshedAt","IsAuthoritativeSource","HasDataQualityScore","ClassificationPassCount","HasAccessEntitlement","HasDataShareAgreementSetOrExempt","GlossaryTermCount")
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
