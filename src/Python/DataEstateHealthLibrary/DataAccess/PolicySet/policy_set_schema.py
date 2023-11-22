from pyspark.sql.types import *

class PolicySetSchema:
    
    sensitive_markers_schema = ArrayType(
        StructType(
            [
                StructField("SecurityGroup", StringType(), True),
                StructField("Tags", StringType(), True),
            ]
        )
    )
    
    policies_schema = StructType(
            [
                StructField("sensitiveMarkers", sensitive_markers_schema, True),
            ]
        )
    
    target_resource_schema = StructType(
            [
                StructField("TargetId", StringType(), True),
                StructField("TargetType", StringType(), True),
            ]
        )
