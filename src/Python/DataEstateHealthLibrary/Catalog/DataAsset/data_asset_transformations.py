from pyspark.sql.functions import *

class DataAssetTransformations:
    
    def add_data_asset_id(dataasset_df):
        data_asset_id_added = dataasset_df.withColumn(
            "DataAssetId", col("Source").getField("assetId")
        )

        return data_asset_id_added
