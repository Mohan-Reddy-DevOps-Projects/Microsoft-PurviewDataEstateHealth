from pyspark.sql.functions import *
from pyspark.sql.types import *

class CatalogSchema:
    
    data_product_schema = StructType(
        [
            StructField("Id", StringType(), True),
            StructField("Name", StringType(), False),
            StructField("Domain", StringType(), False),
            StructField("Type", StringType(), True),
            StructField("Description", StringType(), True),
            StructField("BusinessUse", StringType(), True),
            StructField("Contacts", StringType(), True),
            StructField("UpdateFrequency", StringType(), True),
            StructField("TermsOfUse", StringType(), True),
            StructField("Documentation", StringType(), True),
            StructField("SensitivityLabel", StringType(), True),
            StructField("Status", StringType(), False),
            StructField("AdditionalProperties", StringType(), True),
            StructField("SystemData", StringType(), True),
        ]
    )

    business_domain_schema = StructType(
        [
            StructField("Id", StringType(), True),
            StructField("Name", StringType(), False),
            StructField("Description", StringType(), True),
            StructField("ParentId", StringType(), True),
            StructField("Status", StringType(), False),
            StructField("Thumbnail", StringType(), True),
            StructField("SystemData", StringType(), True),
        ]
    )

    term_schema = StructType(
        [
            StructField("Id", StringType(), True),
            StructField("Name", StringType(), False),
            StructField("Description", StringType(), True),
            StructField("Contacts", StringType(), True),
            StructField("ParentId", StringType(), True),
            StructField("Domain", StringType(), False),
            StructField("Status", StringType(), False),
            StructField("SystemData", StringType(), True),
            StructField("IsLeaf", BooleanType(), True),
        ]
    )

    data_asset_schema = StructType(
        [
            StructField("Id", StringType(), True),
            StructField("Name", StringType(), False),
            StructField("Description", StringType(), True),
            StructField("Domain", StringType(), False),
            StructField("Source", StringType(), True),
            StructField("Contacts", StringType(), True),
            StructField("SensitivityLabel", StringType(), True),
            StructField("Classifications", StringType(), True),
            StructField("Lineage", StringType(), True),
            StructField("Schema", StringType(), True),
            StructField("SystemData", StringType(), True),
            StructField("Type", StringType(), False),
            StructField("TypeProperties", StringType(), True),
        ]
    )

    system_data_schema = StructType(
        [
            StructField("lastModifiedAt", TimestampType(), True),
            StructField("lastModifiedBy", StringType(), True),
            StructField("createdAt", TimestampType(), True),
            StructField("createdBy", StringType(), True),
            StructField("expiredAt", TimestampType(), True),
            StructField("expiredBy", StringType(), True),
        ]
    )
    
    contact_item_schema = ArrayType(
        StructType(
            [
                StructField("id", StringType(), False),
                StructField("description", StringType(), True),
            ]
        )
    )
    
    contact_schema = StructType(
        [
            StructField("owner", contact_item_schema, True),
            StructField("expert", contact_item_schema, True),
            StructField("databaseAdmin", contact_item_schema, True),
        ]
    )

    data_product_additional_properties_schema = StructType(
        [
            StructField("assetCount", LongType(), True)
        ]
    )

    business_domain_thummbnail_schema = StructType(
        [
            StructField("color", StringType(), True)
        ]
    )

    data_asset_source_schema = StructType(
        [
            StructField("type", StringType(), False),
            StructField("assetId", StringType(), True),
            StructField("assetType", StringType(), True),
            StructField("lastRefreshedAt", StringType(), True),
            StructField("lastRefreshedBy", StringType(), True),
        ]
    )

    data_asset_schema_entity_schema = ArrayType(
        StructType(
            [
                StructField("type", StringType(), True),
                StructField("name", StringType(), False),
                StructField("description", StringType(), True),
                StructField("classifications", StringType(), True),
            ]
        )
    )

    azure_sql_table_properties_schema = StructType(
        [
            StructField("format", StringType(), False),
            StructField("serverEndpoint", StringType(), False),
            StructField("schemaName", StringType(), False),
            StructField("tableName", StringType(), False),
        ]
    )

    adls_gen2_path_schema = StructType(
        [
            StructField("serverEndpoint", StringType(), False),
            StructField("container", StringType(), False),
            StructField("folderPath", StringType(), True),
            StructField("fileName", StringType(), True),
        ]
    )
