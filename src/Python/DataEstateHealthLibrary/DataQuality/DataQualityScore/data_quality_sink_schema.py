from pyspark.sql.types import *

class DataQualitySinkSchema:
    
    sink_dataquality_schema = StructType(
        [
            StructField('RowId', StringType(), True),
            StructField('AccountId', StringType(), False),
            StructField('BusinessDomainId', StringType(), False),
            StructField('DataProductId', StringType(), False),
            StructField('DataAssetId', StringType(), False),
            StructField('JobId', StringType(), False),
            StructField('QualityScore', DoubleType(), False),
            StructField('ResultedAt', TimestampType(), False)
            ]
        )
    
    sink_asset_dataqualityscore_schema = StructType(
        [
            StructField("DataAssetId", StringType(), False),
            StructField("QualityScore", DoubleType(), False),
            StructField("LastRefreshedAt", TimestampType(), False),
        ]
    )
    
    sink_product_dataqualityscore_schema = StructType(
        [
            StructField("DataProductId", StringType(), False),
            StructField("QualityScore", DoubleType(), False),
            StructField("LastRefreshedAt", TimestampType(), False),
        ]
    )

    sink_domain_dataqualityscore_schema = StructType(
        [
            StructField("BusinessDomainId", StringType(), False),
            StructField("QualityScore", DoubleType(), False),
            StructField("LastRefreshedAt", TimestampType(), False),
        ]
    )
