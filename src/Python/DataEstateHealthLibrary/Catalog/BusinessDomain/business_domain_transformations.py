from pyspark.sql.functions import *

class BusinessdomainTransformations:
    def calculate_has_not_null_description(businessdomain_df):
        has_not_null_description_added = businessdomain_df.withColumn(
            "HasNotNullDescription", when(col("Description").isNotNull() & 
                    ~(col("Description") == ""), 1)
                                 .otherwise(0)
        )

        return has_not_null_description_added

    def calculate_is_root_domain(businessdomain_df):
        is_root_domain_added = businessdomain_df.withColumn(
            "IsRootDomain", when(col("ParentId").isNotNull() &
                    ~(col("ParentId") == ""), 0)
                             .otherwise(1)
        )

        return is_root_domain_added
    
    def calculate_has_valid_owner(businessdomain_df):
        has_valid_owner_added = businessdomain_df.withColumn(
            "HasValidOwner", lit(0)
        )

        return has_valid_owner_added
