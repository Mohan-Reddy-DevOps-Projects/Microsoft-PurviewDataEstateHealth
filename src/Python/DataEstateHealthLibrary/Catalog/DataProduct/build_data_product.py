import sys
from pyspark.sql import SparkSession
from DataEstateHealthLibrary.Shared.merge_helper_function import *
from pyspark.sql.functions import *
from pyspark.sql.types import *
from functools import reduce
from pyspark.sql import DataFrame
import DataEstateHealthLibrary.Catalog.catalog_constants as constants
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.TestData.data_product_sample_data import DataProductSampleData
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_column_functions import DataProductColumnFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_config import DataProductConfig
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions

class BuildDataProduct:

    def build_data_product_schema(dataproduct_df):
        #dataproduct_df = spark.createDataFrame(data=DataProductSampleData.data_product_sample_dat_1,schema=CatalogSchema.data_product_schema)

        """ build data product table """
        dataproduct_df = CatalogColumnFunctions.add_contacts_schema(dataproduct_df)
    
        dataproduct_df = CatalogColumnFunctions.add_system_data_schema(dataproduct_df)
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdAt", "CreatedAt")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"createdby", "CreatedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"expiredAt", "ExpiredAt")
        dataproduct_df = ColumnFunctions.add_new_column_from_col_field(dataproduct_df,"SystemData" ,"expiredBy", "ExpiredBy")
    
        dataproduct_df = DataProductColumnFunctions.add_additional_properties_schema(dataproduct_df)
        dataproduct_df = DataProductColumnFunctions.add_asset_count(dataproduct_df)

        dataproduct_df = dataproduct_df.select("Id","Name","Domain","Type","Description","BusinessUse","UpdateFrequency","TermsOfUse","Documentation","SensitivityLabel","Status","AssetCount","CreatedAt",
                                                       "CreatedBy","ModifiedAt","ModifiedBy","ExpiredAt","ExpiredAt","Owner","Expert","DatabaseAdmin")
        #flattened_data_product_schema.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("<account_id>_data_product_schema")
        return dataproduct_df

    def build_data_product_contact_association(dataproduct_df):
        """we will build it only after building data product"""


        #dataproduct_df = spark.createDataFrame(data=DataProductSampleData.data_product_sample_dat_1,schema=CatalogSchema.data_product_schema)

        """ build data product table """
        dataproduct_owner_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "owner", "Owner", "Contacts")

        dataproduct_expert_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "expert", "Expert", "Contacts")
    
        dataproduct_database_admin_df = CatalogTransformationFunctions.format_contact(dataproduct_df, "databaseAdmin", "DatabaseAdmin", "Contacts")
    
    
        union_df = [dataproduct_owner_df,dataproduct_expert_df,dataproduct_database_admin_df]
        merged_df = reduce(DataFrame.unionAll, union_df)
    
        merged_df = ColumnFunctions.rename_col(merged_df, "Id", "DataProductId")
        data_product_contact_association_df = merged_df.select("DataProductId", "ContactRole", "ContactId", "ContactDescription")
        #final_dataproduct_df.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("data_product_contact_association")
        return data_product_contact_association_df

    def build_data_product_business_domain_association(dataproduct_df):
        """we will build it only after building data product"""

        #dataproduct_df = spark.createDataFrame(data=DataProductSampleData.data_product_sample_dat_1,schema=CatalogSchema.data_product_schema)

        """ build data product table """
        
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Domain", "BusinessDomainId")
        dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Id", "DataProductId")
        #add is primary data product
        data_product_business_domain_association_df = dataproduct_df.select("DataProductId", "BusinessDomainId")
        #final_dataproduct_df.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("data_product_contact_association")
        return data_product_business_domain_association_df
