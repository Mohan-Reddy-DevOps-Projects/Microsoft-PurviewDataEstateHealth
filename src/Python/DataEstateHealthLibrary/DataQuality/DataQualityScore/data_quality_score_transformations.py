from pyspark.sql.functions import *
from pyspark.sql.types import DoubleType
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.dedup_helper_function import DedupHelperFunction

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
        dedup_rdd = dataquality_df.rdd.map(lambda x: (x["JobId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])

        #convert it to dataframe with sample ration 1 so as to avoid error with null column values 
        deduped_qualityscore_df = dedup_rdd3.toDF(sampleRatio=1.0)
        return deduped_qualityscore_df
    
