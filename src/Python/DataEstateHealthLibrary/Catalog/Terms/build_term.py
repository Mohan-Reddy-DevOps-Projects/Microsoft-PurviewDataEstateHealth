from re import S
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
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Catalog.Terms.term_transformations import TermTransformations

class BuildTerm:
    def build_term_schema(term_df):
        """ build data product table """
        term_df = CatalogColumnFunctions.add_contacts_schema(term_df)
    
        term_df = CatalogColumnFunctions.add_system_data_schema(term_df)
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"createdAt", "CreatedAt")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"createdby", "CreatedBy")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"expiredAt", "ExpiredAt")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"expiredBy", "ExpiredBy")
        term_df = ColumnFunctions.rename_col(term_df, "Id", "TermId")
        term_df = TermTransformations.calculate_has_description(term_df)
        term_df = term_df.select("TermId", "Name", "HasDescription", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy", "ExpiredAt", "ExpiredBy", "Status")
        #flattened_data_product_schema.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("<account_id>_data_product_schema")
        return term_df

    def build_term_contact_association(term_df):
        term_owner_df = CatalogTransformationFunctions.format_contact(term_df, "owner", "Owner", "Contacts")

        term_expert_df = CatalogTransformationFunctions.format_contact(term_df, "expert", "Expert", "Contacts")
    
        term_database_admin_df = CatalogTransformationFunctions.format_contact(term_df, "databaseAdmin", "DatabaseAdmin", "Contacts")
    
        union_df = [term_owner_df,term_expert_df,term_database_admin_df]
        merged_df = reduce(DataFrame.unionAll, union_df)
        merged_df = ColumnFunctions.rename_col(term_df, "Id", "TermId")
        term_contact_association_df = merged_df.select("TermId", "ContactRole", "ContactId", "ContactDescription")
        return term_contact_association_df

    def build_term_business_domain_association_schema(term_df):
        """ build data product table """

        term_df = ColumnFunctions.rename_col(term_df, "Domain", "BusinessDomainId")
        term_df = ColumnFunctions.rename_col(term_df, "Id", "TermId")
        
        term_business_domain_association_df = term_df.select("BusinessDomainId", "TermId")
        return term_business_domain_association_df
