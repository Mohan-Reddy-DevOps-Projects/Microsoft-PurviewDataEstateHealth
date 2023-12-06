from pyspark.sql.functions import *
from DataEstateHealthLibrary.DataQuality.DataQualityFact.data_quality_fact_schema import DataQualityFactSchema

class DataQualityFactColumnFunctions:
        
        def add_id_schema(dataqualityfact_df):
            id_schema_added = dataqualityfact_df.withColumn(
                "ResultId", from_json(col("ResultId"), DataQualityFactSchema.data_quality_id_schema)
            )

            return id_schema_added
