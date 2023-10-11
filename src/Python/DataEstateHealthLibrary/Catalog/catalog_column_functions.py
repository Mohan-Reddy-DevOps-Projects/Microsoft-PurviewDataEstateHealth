from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema

class CatalogColumnFunctions:
   
    def add_system_data_schema(catalog_df):
        system_data_schema_added = catalog_df.withColumn(
            "SystemData", from_json(col("SystemData"), CatalogSchema.system_data_schema)
        )

        return system_data_schema_added

    def add_contacts_schema(catalog_df):
        contacts_schema_added = catalog_df.withColumn(
            "Contacts", from_json(col("Contacts"), CatalogSchema.contact_schema)
        )

        return contacts_schema_added
