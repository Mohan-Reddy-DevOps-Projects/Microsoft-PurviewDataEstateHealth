from pyspark.sql.functions import *
from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema

class BusinessDomainColumnFunctions:
     def add_thumbnail_schema(businessdomain_df):
        thumbnail_schema_added = businessdomain_df.withColumn(
            "Thumbnail", from_json(col("Thumbnail"), CatalogSchema.business_domain_thummbnail_schema)
        )

        return thumbnail_schema_added
