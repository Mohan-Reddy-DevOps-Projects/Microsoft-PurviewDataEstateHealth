from pyspark.sql.functions import *

class BusinessdomainTransformations:
    def calculate_has_not_null_description(businessdomain_df):
        has_not_null_description_added = businessdomain_df.withColumn(
            "HasNotNullDescription", when(col("Description").isNotNull() & 
                    ~(col("Description") == ""), True)
                                 .otherwise(False)
        )

        return has_not_null_description_added

    def calculate_is_root_domain(businessdomain_df):
        is_root_domain_added = businessdomain_df.withColumn(
            "IsRootDomain", when(col("ParentId").isNotNull() &
                    ~(col("ParentId") == ""), False)
                             .otherwise(True)
        )

        return is_root_domain_added
