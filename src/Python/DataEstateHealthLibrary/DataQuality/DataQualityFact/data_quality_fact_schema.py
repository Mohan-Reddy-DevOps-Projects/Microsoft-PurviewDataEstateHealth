from pyspark.sql.types import *
class DataQualityFactSchema:
        data_quality_id_schema = StructType(
        [
            StructField("businessDomainId", StringType(), False),
            StructField("dataProductId", StringType(), False),
            StructField("dataAssetId", StringType(), False),
            StructField("jobId", StringType(), False),
        ]
    )
