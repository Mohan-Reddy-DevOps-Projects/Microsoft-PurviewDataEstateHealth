from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_column_functions import BusinessDomainColumnFunctions
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_transformations import DataProductTransformations
from DataEstateHealthLibrary.Catalog.Terms.term_transformations import TermTransformations
from DataEstateHealthLibrary.Shared.dedup_helper_function import DedupHelperFunction
from pyspark.sql.functions import *
from pyspark.sql.types import *

from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class BuildBusinessDomain:

    def build_business_domain_schema(businessdomain_df, dataproduct_domain_association_df, term_domain_association_df):

        businessdomain_df = CatalogColumnFunctions.add_system_data_schema(businessdomain_df)
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"createdAt", "CreatedAt")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"createdBy", "CreatedBy")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        
        businessdomain_df = BusinessdomainTransformations.calculate_has_valid_owner(businessdomain_df)
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "Name", "BusinessDomainDisplayName")
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "Id", "BusinessDomainId")
        businessdomain_df = BusinessdomainTransformations.calculate_is_root_domain(businessdomain_df)
        businessdomain_df = BusinessdomainTransformations.calculate_has_description(businessdomain_df)
        businessdomain_df = HelperFunction.calculate_last_refreshed_at(businessdomain_df)

        #get count from data product and term
        businessdomain_dataproduct_count = CatalogColumnFunctions.calculate_count_for_domain(dataproduct_domain_association_df, "DataProductsCount")
        businessdomain_term_count = CatalogColumnFunctions.calculate_count_for_domain(term_domain_association_df, "GlossaryTermsCount")
        #join both counts
        count_df = businessdomain_dataproduct_count.join(businessdomain_term_count,"BusinessDomainId","outer").drop(businessdomain_term_count.BusinessDomainId)
        #join with bussiness domain dataframe
        businessdomain_df = businessdomain_df.join(count_df,"BusinessDomainId","leftouter")

        #add timestamp for deduping
        businessdomain_df = CatalogTransformationFunctions.add_timestamp_col(businessdomain_df)
        #remove duplicate rows
        businessdomain_df = businessdomain_df.distinct()
        
        #map by business domain id and reduce by timestamp
        dedup_rdd = businessdomain_df.rdd.map(lambda x: (x["BusinessDomainId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])
        
        #convert it to dataframe with sample ration 1 so as to avoid error with null column values
        final_businessdomain_df = dedup_rdd3.toDF(sampleRatio=1.0)
        
        final_businessdomain_df = final_businessdomain_df.select("BusinessDomainId", "BusinessDomainDisplayName", "HasDescription", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy", "GlossaryTermsCount",
                                                     "DataProductsCount", "IsRootDomain","HasValidOwner", "LastRefreshedAt")
        return final_businessdomain_df
    
    def build_catalog_association(businessdomain_df,assetproduct_association, termdomain_association_df, productdomain_association):

        businessdomain_df = businessdomain_df.select("Id")
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "Id", "BusinessDomainId")
        #joins
        businessdomain_df = businessdomain_df.join(productdomain_association,"BusinessDomainId","fullouter")
        businessdomain_df = businessdomain_df.join(assetproduct_association,"BusinessDomainId","fullouter")
        final_df_association = businessdomain_df.join(termdomain_association_df,"BusinessDomainId","fullouter")
        #remove duplicate rows
        final_df_association = final_df_association.distinct()
        #drop any row which has a null value
        #final_df_association = final_df_association.na.drop()
        return final_df_association
