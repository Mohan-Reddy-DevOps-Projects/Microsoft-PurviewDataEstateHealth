from pyspark.sql.functions import *

class DataQualityFactTransformations:
        
        def add_job_id(dataqualityfact_df):
            added_job_id = dataqualityfact_df.withColumn(
            "JobId", when(col("ResultId").isNotNull(),
                                dataqualityfact_df.ResultId.getField("jobId")))
            return added_job_id
