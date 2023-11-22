from pyspark.sql.functions import *
class PolicySetTransformations:
    
    def add_target_type(policyset_df):
        target_type_added = policyset_df.withColumn(
            "TargetType", col("TargetResource").getField("TargetType")
            )
        return target_type_added
        
    def add_dataproduct_id(policyset_df):
        dataproduct_id_added = policyset_df.withColumn(
            "DataProductId", col("TargetResource").getField("TargetId")
            )
        return dataproduct_id_added
        
    def add_sensitive_markers(policyset_df):
        sensitive_markers_added = policyset_df.withColumn(
            "SensitiveMarkers", col("Policies").getField("sensitiveMarkers")
        )

        return sensitive_markers_added
        
    def calculate_has_access_entitlement(policyset_df):
        has_access_added = policyset_df.withColumn(
            "HasAccessEntitlement", when(col("SensitiveMarkers").isNotNull() &
                                         (size(col("SensitiveMarkers")) > 0) , True)
            .otherwise(False))                                
        return has_access_added
