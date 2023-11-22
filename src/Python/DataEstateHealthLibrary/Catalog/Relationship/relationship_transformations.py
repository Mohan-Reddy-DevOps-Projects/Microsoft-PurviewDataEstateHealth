from pyspark.sql.functions import *

class RelationshipTransformations:
     
    def calculate_asset_count(dataframe):
        dataframe = dataframe.groupBy("DataProductId").count()
        dataframe = dataframe.withColumnRenamed("count","AssetCount")
        return dataframe

    def calculate_dataasset_id(relationship_df):
        assetid_added = relationship_df.withColumn(
            "DataAssetId",when(col("SourceType") == "DataAsset", col("SourceId"))
            .otherwise(col("TargetId"))
            )
        return assetid_added
    
    def calculate_dataproduct_id(relationship_df):
        productid_added = relationship_df.withColumn(
            "DataProductId",when(col("SourceType") == "DataProduct", col("SourceId"))
            .otherwise(col("TargetId"))
            )
        return productid_added
