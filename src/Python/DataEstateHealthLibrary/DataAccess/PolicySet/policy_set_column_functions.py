from pyspark.sql.functions import *
from DataEstateHealthLibrary.DataAccess.PolicySet.policy_set_schema import PolicySetSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions

class PolicySetColumnFunctions:
    def add_target_resource_schema(policyset_df):
        target_resource_schema_added = policyset_df.withColumn(
            "TargetResource", from_json(col("TargetResource"), PolicySetSchema.target_resource_schema)
        )

        return target_resource_schema_added
    
    def add_policies_schema(policyset_df):
        policies_schema_added = policyset_df.withColumn(
            "Policies", from_json(col("Policies"), PolicySetSchema.policies_schema)
        )

        return policies_schema_added
