from pyspark.sql.functions import *
from pyspark.sql import Window
from pyspark.sql.functions import col, row_number
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class DataQualityScoreTransformations:
    def calculate_has_data_quality_score(dataquality_df):
        has_data_quality_score_added = dataquality_df.withColumn(
            "HasDataQualityScore", lit(True)
        )

        return has_data_quality_score_added

    def calculate_total_count_col(dataquality_df):
        total_count_col_added = dataquality_df.withColumn(
            "TotalCount", lit(1)
        )

        return total_count_col_added
    
    def calculate_column_sum(dataquality_df, groupByColumnName):
        dataquality_df = dataquality_df.groupBy(groupByColumnName).sum()
        dataquality_df = dataquality_df.withColumnRenamed("sum(QualityScore)","QualityScore")
        dataquality_df = dataquality_df.withColumnRenamed("sum(TotalCount)","TotalCount")
        return dataquality_df
    
    def calculate_score(dataquality_df):
        score_added = dataquality_df.withColumn(
        "QualityScore", lit((col("QualityScore")/col("TotalCount")))
        )

        return score_added
    
    def dedup_quality_score(dataquality_df):
        #add timestamp for deduping
        dataquality_df = HelperFunction.add_timestamp_col(dataquality_df,"ResultedAt")
        #remove duplicate rows
        dataquality_df = dataquality_df.distinct()
        
        #map by job id and reduce by timestamp
        window_spec = Window.partitionBy("JobId").orderBy(col("Timestamp").desc())
        deduped_qualityscore_df = dataquality_df.withColumn("row_num", row_number().over(window_spec)).filter("row_num = 1").drop("row_num")

        return deduped_qualityscore_df
    
