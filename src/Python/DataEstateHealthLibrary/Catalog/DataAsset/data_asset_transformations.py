from pyspark.sql.functions import *

class DataAssetTransformations:
    
    def add_data_asset_id(dataasset_df):
        data_asset_id_added = dataasset_df.withColumn(
            "SourceAssetId", col("Source").getField("assetId")
        )

        return data_asset_id_added
    
    def add_source_type(dataasset_df):
        source_type_added = dataasset_df.withColumn(
            "SourceType", col("Source").getField("assetType")
        )

        return source_type_added
    
    def calculate_has_classification(dataasset_df):
        has_classification_added = dataasset_df.withColumn(
            "HasClassification",  when(col("Classifications").isNotNull(), True)
            .otherwise(False)
        ) 

        return has_classification_added
    
    def calculate_has_schema(dataasset_df):
        has_schema_added = dataasset_df.withColumn(
            "HasSchema",  when(col("Schema").isNotNull(), True)
            .otherwise(False)
        )

        return has_schema_added
    
    def calculate_has_description(dataasset_df):
        has_description_added = dataasset_df.withColumn(
            "HasDescription", when(col("Description").isNotNull(), True)
            .otherwise(False)
        )

        return has_description_added
    
    def calculate_sum_for_classification_count(dataframe):
        dataframe = dataframe.groupBy("DataProductId").sum()
        dataframe = dataframe.withColumnRenamed("sum(HasClassification)","ClassificationPassCount")
        return dataframe
    

