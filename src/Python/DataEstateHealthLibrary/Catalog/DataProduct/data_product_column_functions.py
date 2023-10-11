from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema

class DataProductColumnFunctions:
    
    def add_additional_properties_schema(dataproduct_df):
        additional_properties_schema_added = dataproduct_df.withColumn(
            "AdditionalProperties", from_json(col("AdditionalProperties"), CatalogSchema.data_product_additional_properties_schema)
        )

        return additional_properties_schema_added

    def add_asset_count(catalog_df):
        asset_count_added = catalog_df.withColumn(
            "AssetCount", col("AdditionalProperties").getField("assetCount")
        )

        return asset_count_added
