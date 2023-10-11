from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_types import DataAssetTypes
from pyspark.sql.functions import *
from pyspark.sql.types import *

class DataAssetColumnFunctions:

    def add_source_schema(dataasset_df):
        source_schema_added = dataasset_df.withColumn(
            "Source", from_json(col("Source"), CatalogSchema.data_asset_source_schema)
        )

        return source_schema_added

    def add_schema_entity_schema_added(dataasset_df):
        schema_entity_schema_added = dataasset_df.withColumn(
            "Schema", from_json(col("Schema"), CatalogSchema.data_asset_schema_entity_schema)
        )

        return schema_entity_schema_added

    def add_azure_sql_table_type_properties_schema(dataasset_df):
        azure_sql_table_type_properties_schema_added = dataasset_df.withColumn(
            "TypeProperties", from_json(col("TypeProperties"), CatalogSchema.azure_sql_table_properties_schema)
        )

        return azure_sql_table_type_properties_schema_added

    def add_adls_gen2_path_schema(dataasset_df):
        adls_gen2_path_schema_added = dataasset_df.withColumn(
            "TypeProperties", from_json(col("TypeProperties"), CatalogSchema.adls_gen2_path_schema)
        )

        return adls_gen2_path_schema_added

    def add_type_properties_scehma(dataasset_df):
        type_properties_schema_added = dataasset_df.withColumn(
            "TypeProperties", when(col("Type") == "AzureSqlTable",
                                   DataAssetColumnFunctions.add_azure_sql_table_type_properties_schema(dataasset_df))
                              .otherwise(DataAssetColumnFunctions.add_adls_gen2_path_schema(dataasset_df))
            )                  
        
