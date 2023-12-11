from pyspark.sql.functions import *
from pyspark.sql import Window
from pyspark.sql.functions import col, row_number
from DataEstateHealthLibrary.DataAccess.PolicySet.policy_set_column_functions import PolicySetColumnFunctions
from DataEstateHealthLibrary.DataAccess.PolicySet.policy_set_transformations import PolicySetTransformations
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class BuildPolicySet:
    def build_policy_set(policyset_df):
        
        policyset_df = PolicySetColumnFunctions.add_target_resource_schema(policyset_df)
        policyset_df = PolicySetTransformations.add_target_type(policyset_df)
        
        #filter only data product types
        policyset_df = policyset_df.filter(col("PolicySetKind") == "DataProduct")
        #filter only data product types
        policyset_df = policyset_df.filter(col("TargetType") == "DataProduct")
        
        policyset_df = PolicySetTransformations.add_dataproduct_id(policyset_df)
        
        policyset_df = PolicySetColumnFunctions.add_policies_schema(policyset_df)
        policyset_df = PolicySetTransformations.add_sensitive_markers(policyset_df)
        policyset_df = PolicySetTransformations.calculate_has_access_entitlement(policyset_df)
        
        #add timestamp for deduping
        policyset_df = HelperFunction.add_timestamp_col(policyset_df, "ModifiedAt")
        
        #remove duplicate rows
        policyset_df = policyset_df.distinct()
        
        #map by data product id and reduce by timestamp
        window_spec = Window.partitionBy("DataProductId").orderBy(col("Timestamp").desc())
        final_policyset_df = policyset_df.withColumn("row_num", row_number().over(window_spec)).filter("row_num = 1").drop("row_num")

        final_policyset_df = final_policyset_df.select("DataProductId", "HasAccessEntitlement")
        return final_policyset_df
    
    def handle_policy_set_deletes(policyset_df, delete_policyset_df):
        if not delete_policyset_df.isEmpty():
            policyset_df = policyset_df.join(delete_policyset_df, ["Id"], "leftanti")    
        return policyset_df
