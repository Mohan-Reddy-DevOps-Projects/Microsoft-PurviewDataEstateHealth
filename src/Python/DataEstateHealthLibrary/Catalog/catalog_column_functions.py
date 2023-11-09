from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

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

    def calculate_count_for_domain(domain_association_df, colName):
        count_added = domain_association_df.groupBy("BusinessDomainId").count()
        count_added = ColumnFunctions.rename_col(count_added, "count", colName)
        count_added = count_added.withColumn(colName, when(col(colName).isNull(), lit(0))
            .otherwise(lit(col(colName)))
               )
        return count_added
