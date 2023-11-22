from pyspark.sql.functions import *

class BusinessdomainTransformations:
    def calculate_has_description(businessdomain_df):
        has_description_added = businessdomain_df.withColumn(
            "HasDescription", when(col("Description").isNotNull() & 
                    ~(col("Description") == ""), True)
                                 .otherwise(False)
        )

        return has_description_added

    def calculate_is_root_domain(businessdomain_df):
        is_root_domain_added = businessdomain_df.withColumn(
            "IsRootDomain", when(col("ParentId").isNotNull() &
                    ~(col("ParentId") == ""), False)
                             .otherwise(True)
        )

        return is_root_domain_added
    
    def calculate_has_valid_owner(businessdomain_df):
        has_valid_owner_added = businessdomain_df.withColumn(
            "HasValidOwner", lit(False)
        )

        return has_valid_owner_added
    
    def calculate_default_dataproducts_count(businessdomain_df):
        dataproducts_count_added = businessdomain_df.withColumn(
            "DataProductsCount", lit(0)
        )

        return dataproducts_count_added
    
    def calculate_default_glossaryterms_count(businessdomain_df):
        glossaryterms_count_added = businessdomain_df.withColumn(
            "GlossaryTermsCount", lit(0)
        )

        return glossaryterms_count_added
