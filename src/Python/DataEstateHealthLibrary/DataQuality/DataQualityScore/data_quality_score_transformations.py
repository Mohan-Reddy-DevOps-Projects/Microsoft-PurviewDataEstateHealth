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

    def calculate_product_score(dataquality_df):
        dataquality_df = dataquality_df.groupBy("DataProductId", "BusinessDomainId").agg(avg("QualityScore"), count("*"))
        dataquality_df = dataquality_df.withColumnRenamed("avg(QualityScore)","QualityScore")

        return dataquality_df
        
    def calculate_domain_score(dataquality_df):
        dataquality_df = dataquality_df.groupBy("BusinessDomainId").agg(avg("QualityScore"), count("*"))
        dataquality_df = dataquality_df.withColumnRenamed("avg(QualityScore)","QualityScore")

        return dataquality_df
    
    def dedup_quality_score(dataquality_df):
        #add timestamp for deduping
        dataquality_df = HelperFunction.add_timestamp_col(dataquality_df,"ResultedAt")
        #remove duplicate rows
        dataquality_df = dataquality_df.distinct()
        
        #map by job id and reduce by timestamp
        window_spec = Window.partitionBy("JobId").orderBy(col("Timestamp").desc())
        deduped_qualityscore_df = dataquality_df.withColumn("row_num", row_number().over(window_spec)).filter("row_num = 1").drop("row_num")

        return deduped_qualityscore_df
    
    def dedup_quality_score_by_identifiers(dataquality_df):
        #remove duplicate rows
        dataquality_df = dataquality_df.distinct()
        
        #map by job id and reduce by timestamp
        window_spec = Window.partitionBy("DataProductId","BusinessDomainId","DataAssetId").orderBy(col("Timestamp").desc())
        deduped_qualityscore_df = dataquality_df.withColumn("row_num", row_number().over(window_spec)).filter("row_num = 1").drop("row_num")

        return deduped_qualityscore_df
    
