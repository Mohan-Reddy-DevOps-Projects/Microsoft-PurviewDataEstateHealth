from pyspark.sql.functions import *
from pyspark.sql.types import *

class AtlasRddScehmas:

    data_asset_schema = StructType(
        [
            StructField("CollectionId", StringType(), True),
            StructField("DataAssetId", StringType(), True),
            StructField("CurationLevel", StringType(), True),
            StructField("DataAssetDescription", StringType(), True),
            StructField("DataAssetDisplayName", StringType(), True),
            StructField("HasGlossaryTerm", BooleanType(), True),
            StructField("HasManualClassification", BooleanType(), True),
            StructField("HasScannedClassification", BooleanType(), True),
            StructField("HasSchema", BooleanType(), True),
            StructField("HasSensitiveClassification", BooleanType(), True),
            StructField("HasValidDescription", BooleanType(), True),
            StructField("HasValidOwner", StringType(), True),
            StructField("CreatedAt", LongType(), True),
            StructField("CreatedBy", StringType(), True),
            StructField("ModifiedAt", LongType(), True),
            StructField("ModifiedBy", LongType(), True),
            StructField("ObjectType", StringType(), True),
            StructField("Platform", StringType(), True),
            StructField("Provider", StringType(), True),
             StructField("QualifiedName", StringType(), True),
            StructField("SourceInstance", StringType(), True),
            StructField("SourceType", StringType(), True),
            StructField("UnclassificationReason", StringType(), True),
        ]
    )

    atlas_rdd_schema = StructType(
        [
        StructField("Guid", StringType(), True),
        StructField("Status", StringType(), True),
        StructField("Version", FloatType(), True),
        StructField("Timestamp", LongType(), True),
        StructField("LastModifiedTS", StringType(), True),
        StructField("Attributes", StringType(), True),
        StructField("TypeName", StringType(), True),
        StructField("Classifications", StringType(), True),
        StructField("InputToProcesses", StringType(), True),
        StructField("OutputFromProcesses", StringType(), True),
        StructField("CatalogId", StringType(), True),
        StructField("AccountId", StringType(), True),
        StructField("QualifiedName", StringType(), True),
        StructField("ResourceSetUri", StringType(), True),
        StructField("ParentGuid", StringType(), True),
        StructField("SchemaEntities", StringType(), True),
        StructField("RelationshipAttributes", StringType(), True),
        StructField("Source", StringType(), True),
        StructField("SourceDetails", StringType(), True),
        StructField("CollectionSource", StringType(), True),
        StructField("CollectionId", StringType(), True),
        StructField("CollectionPath", StringType(), True),
        StructField("Contacts", StringType(), True),
        StructField("EntityInternalData", StringType(), True),
        StructField("ExtendedProps", StringType(), True),
        StructField("CreateTime", LongType(), True),
        StructField("CreatedBy", StringType(), True),
        StructField("UpdateTime", LongType(), True),
        StructField("UpdatedBy", StringType(), True),
        StructField("SensitivityLabel", StringType(), True),
        StructField("StoredTier", StringType(), True),
        StructField("BucketNumber", IntegerType(), True),
        ]
    )

    classification_scehma = StructType(
        [
            StructField("Category", StringType(), True),
            StructField("ClassificationId", StringType(), True),
            StructField("ClassificationType", StringType(), True),
            StructField("Name", StringType(), True),
            StructField("DisplayName", StringType(), True),
            StructField("Description", StringType(), True),
        ]
    )

    label_schema = StructType(
        [
            StructField("LinkedAssetId", StringType(), True),
            StructField("LinkedAssetType", StringType(), True),
            StructField("LabelId ", StringType(), True),
        ]
    )

    collection_schema = StructType(
        [
            StructField("CollectionId", StringType(), True),
            StructField("FriendlyPath", StringType(), True),
            StructField("FriendlyName ", StringType(), True),
            StructField("CreatedAt ", DateType(), True),
            StructField("CreatedBy ", StringType(), True),
            StructField("ModifiedAt ", DateType(), True),
            StructField("ModifiedBy ", StringType(), True),
        ]
    )
    
    data_asset_column_schema = StructType(
        [
            StructField("DataAssetId", StringType(), True),
            StructField("DataAssetColumnId", StringType(), True),
            StructField("QualifiedName", StringType(), True),
            StructField("DisplayName", StringType(), True),
            StructField("HasValidDescription", BooleanType(), True),
            StructField("HasUserDescription", BooleanType(), True),
            StructField("HasClassification", BooleanType(), True),
            StructField("HasTerms", BooleanType(), True),
        ]
    )

    classifiaction_association_schema = StructType(
        [
            StructField("ClassificationId", StringType(), True),
            StructField("ClassificationSource", StringType(), True),
            StructField("DataAssetColumnId", StringType(), True),
            StructField("DataAssetId", StringType(), True),
        ]
    )

    meanings_json_schema = StructType(
        StructType(
            [
                StructField("terms", StringType(), True)
            ]
        )
    )

    relationship_attributes_json_schema = StructType(
        StructType(
            [
                StructField("meanings", meanings_json_schema, True)
            ]
        )
    )
    
    source_details_json_schema = StructType(
        [
        StructField("MCE_RuleId", StringType(), True),
        StructField("ClassificationRuleType", StringType(), True),
        ]
    )
    
    attributes_json_schema = StructType(
        [
            StructField("qualifiedName", StringType(), True),
            StructField("guid", StringType(), True),
            StructField("displayName", StringType(), True),
            StructField("name", StringType(), True),
            StructField("description", StringType(), True),
            StructField("userDescription", StringType(), True),
            StructField("isFile", BooleanType(), True),
            StructField("isBlob", BooleanType(), True),
        ]
    )

    attributes_json_schema2 = StructType(
        [
            StructField("qualifiedName", StringType(), True),
            StructField("guid", StringType(), True),
            StructField("displayName", StringType(), True),
            StructField("name", StringType(), True),
            StructField("description", StringType(), True),
            StructField("userDescription", StringType(), True),
            StructField("isFile", BooleanType(), True),
            StructField("isBlob", BooleanType(), True),
        ]
    )
    
    classification_json_schema = ArrayType(
        StructType(
            [
                StructField("typeName", StringType(), True),
                StructField("source", StringType(), True),
                StructField("sourceDetails", source_details_json_schema, True),
                StructField("attributes", attributes_json_schema, True),
            ]
        )
    )

    schema_entities_json_schema = ArrayType(
        StructType(
            [
                StructField("classifications", classification_json_schema, True),
                StructField("attributes", attributes_json_schema, True),
                StructField("guid", StringType(), True),
                StructField("relationshipAttributes", relationship_attributes_json_schema, True),
            ]
        )
    )

    data_owner_json_schema = ArrayType(
        StructType(
            [
                StructField("id", StringType(), False)
            ]
        )
    )
