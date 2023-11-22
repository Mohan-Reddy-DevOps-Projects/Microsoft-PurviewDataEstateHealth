from re import S
import sys
from pyspark.sql import SparkSession
from pyspark.sql.functions import *
from pyspark.sql.types import *
from functools import reduce
from pyspark.sql import DataFrame
import DataEstateHealthLibrary.Catalog.catalog_constants as constants
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_column_functions import DataProductColumnFunctions
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Catalog.Terms.term_transformations import TermTransformations
from DataEstateHealthLibrary.Shared.dedup_helper_function import DedupHelperFunction
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction


class BuildTerm:
    
    def build_term_schema(term_df):
        
        term_df = CatalogColumnFunctions.add_system_data_schema(term_df)
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"createdAt", "CreatedAt")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"createdBy", "CreatedBy")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        term_df = ColumnFunctions.add_new_column_from_col_field(term_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        
        term_df = ColumnFunctions.rename_col(term_df, "Id", "TermId")
        term_df = HelperFunction.calculate_last_refreshed_at(term_df,"LastRefreshedAt")
        term_df = TermTransformations.calculate_has_description(term_df)

        #add timestamp for deduping
        term_df = CatalogTransformationFunctions.add_timestamp_col(term_df)
        #remove duplicate rows
        term_df = term_df.distinct()
        
        #map by term id and reduce by timestamp
        dedup_rdd = term_df.rdd.map(lambda x: (x["TermId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])
        
        #convert it to dataframe with sample ration 1 so as to avoid error with null column values 
        final_term_df = dedup_rdd3.toDF(sampleRatio=1.0)
        
        final_term_df = final_term_df.select("TermId", "Name","Status", "HasDescription", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy", "LastRefreshedAt")
        return final_term_df

    def build_term_contact_association(term_df):
        term_df = CatalogColumnFunctions.add_contacts_schema(term_df)
        term_owner_df = CatalogTransformationFunctions.format_contact(term_df, "owner", "Owner", "Contacts")

        term_expert_df = CatalogTransformationFunctions.format_contact(term_df, "expert", "Expert", "Contacts")
    
        term_database_admin_df = CatalogTransformationFunctions.format_contact(term_df, "databaseAdmin", "DatabaseAdmin", "Contacts")
    
        union_df = [term_owner_df,term_expert_df,term_database_admin_df]
        merged_df = reduce(DataFrame.unionAll, union_df)
        
        term_contact_association_df = ColumnFunctions.rename_col(merged_df, "Id", "TermId")
        term_contact_association_df = term_contact_association_df.select("TermId", "ContactRole", "ContactId", "ContactDescription")
        
        #remove duplicate rows
        term_contact_association_df = term_contact_association_df.distinct()
        #drop any row which has a null value in listed columns
        term_contact_association_df = term_contact_association_df.na.drop(subset=["TermId","ContactId","ContactRole"])
        return term_contact_association_df

    def build_term_business_domain_association_schema(term_df):
        term_df = ColumnFunctions.rename_col(term_df, "Domain", "BusinessDomainId")
        term_df = ColumnFunctions.rename_col(term_df, "Id", "TermId")
        
        term_business_domain_association_df = term_df.select("BusinessDomainId", "TermId")
        
        #remove duplicate rows
        term_business_domain_association_df = term_business_domain_association_df.distinct()
        #drop any row which has a null value
        term_business_domain_association_df = term_business_domain_association_df.na.drop()
        return term_business_domain_association_df

    def handle_term_deletes(term_df, deleted_term_df):
        if not deleted_term_df.isEmpty():
            deleted_term_df = deleted_term_df.select("Id");
            term_df = term_df.join(deleted_term_df, ["Id"], "leftanti")
            
        return term_df
    
    def update_boolean_to_int(term_df):
        term_df = HelperFunction.update_col_to_int(term_df,"HasDescription")
            
        return term_df
