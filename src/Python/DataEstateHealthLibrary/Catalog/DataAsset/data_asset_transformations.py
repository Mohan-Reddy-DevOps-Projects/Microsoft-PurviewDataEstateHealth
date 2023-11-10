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
            "HasClassification",  when(col("Classifications").isNotNull(), 1)
            .otherwise(0)
        ) 

        return has_classification_added
    
    def calculate_has_schema(dataasset_df):
        has_schema_added = dataasset_df.withColumn(
            "HasSchema",  when(col("Schema").isNotNull(), 1)
            .otherwise(0)
        )

        return has_schema_added
    
    def calculate_has_description(dataasset_df):
        has_description_added = dataasset_df.withColumn(
            "HasDescription", when(col("Description").isNotNull(), 1)
            .otherwise(0)
        )

        return has_description_added
    
    def calculate_sum_for_classification_count(dataasset_df):
        dataasset_df = dataasset_df.groupBy("DataProductId").sum()
        dataasset_df = dataasset_df.withColumnRenamed("sum(HasClassification)","HasClassification")
        return dataasset_df
