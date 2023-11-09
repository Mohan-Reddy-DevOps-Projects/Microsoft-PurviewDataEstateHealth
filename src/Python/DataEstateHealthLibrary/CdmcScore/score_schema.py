from pyspark.sql.functions import *
from pyspark.sql.types import *

class ScoreSchema:

    score_schema = StructType([
        StructField("BusinessDomainId", StringType(), False),
        StructField("TotalDataProducts", IntegerType(), False),
        StructField("C2_Ownership_Total", IntegerType(), False),
        StructField("C3_AuhoritativeSource_Total", IntegerType(), False),
        StructField("C5_Catalog_Total", IntegerType(), False),
        StructField("C6_Classification_Total", IntegerType(), False),
        StructField("C7_Access_Total", IntegerType(), False),
        StructField("C8_DataConsumptionPurpose_Total", IntegerType(), False),
        StructField("C12_Quality_Total", IntegerType(), False),
        StructField("DataHealth", DoubleType(), False),
        StructField("MetadataCompleteness", DoubleType(), False),
        StructField("Use", DoubleType(), False),
        StructField("Quality", DoubleType(), False),
        StructField("ActualValue", DoubleType(), False),
        ]
    )
    
    final_score_schema = StructType(
        [
            StructField("Name", StringType(), False),
            StructField("Description", StringType(), False),
            StructField("Kind", StringType(), True),
            StructField("ActualValue", IntegerType(), True),
        ]
    )
