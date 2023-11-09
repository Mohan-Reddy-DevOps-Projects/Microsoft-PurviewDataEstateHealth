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
    
    def calculate_has_not_null_description(dataasset_df):
        has_not_null_description_added = dataasset_df.withColumn(
            "HasNotNullDescription", when(col("Description").isNotNull(), 1)
            .otherwise(0)
        )

        return has_not_null_description_added
