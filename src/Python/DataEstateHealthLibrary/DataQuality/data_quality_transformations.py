from pyspark.sql.functions import *

class DataQualityTransformations:
    def calculate_has_data_quality_score(dataquality_df):
        has_data_quality_score_added = dataquality_df.withColumn(
            "HasDataQualityScore", lit(True)
        )

        return has_data_quality_score_added
