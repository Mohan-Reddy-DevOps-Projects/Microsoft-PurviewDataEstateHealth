from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_schemas import AtlasRddScehmas
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_column_functions import AtlasRddColumnFunctions
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_transformations import AtlasRddTransformations
from DataEstateHealthLibrary.Shared.merge_helper_function import MergeHelperFunction
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import get_json_object,col

class BuildAtlasRdd:
    
    def build_atlas_rdd(atlas_rdd_df):
        
        """Dedup : Merge assets having multiple instances based on timestamp. Will keep the latest instance of the asset"""
        merged_atlas_rdd = atlas_rdd_df.rdd.map(lambda x: (x["Guid"], x))
        merged_atlas_rdd2 = merged_atlas_rdd.reduceByKey(lambda x,y : MergeHelperFunction.assets_reduce_func(x,y))
        merged_atlas_rdd3 = merged_atlas_rdd2.map(lambda x: x[1])

        final_atlas_rdd = merged_atlas_rdd3.toDF(sampleRatio=1.0)
        return final_atlas_rdd
    
    def build_classification_schema(atlas_rdd_df):
        atlas_rdd_df = atlas_rdd_df.select("Guid", "Classifications")

        atlas_rdd_df = AtlasRddColumnFunctions.compute_classification(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_classification_attribute_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_display_name(atlas_rdd_df, "ClassificationAttributes", "DisplayName")
        atlas_rdd_df = AtlasRddColumnFunctions.add_description(atlas_rdd_df, "ClassificationAttributes", "Description")
        atlas_rdd_df = AtlasRddColumnFunctions.add_classification_rule_type(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_classification_rule_id(atlas_rdd_df)

        classification_df = atlas_rdd_df.select("Category", "ClassificationId", "ClassificationType", "Name", "DisplayName", "Description")
        return classification_df

    def build_ladel_schema(atlas_rdd_df):
        
        """ filter rows which have sensitivity label value as null. only keep non null ones """
        atlas_rdd_df_filtered = atlas_rdd_df.select("Guid", "TypeName", "SensitivityLabel").filter(col("SensitivityLabel").isNotNull())
        """ rename guid column """
        atlas_rdd_df_filtered = ColumnFunctions.rename_col(atlas_rdd_df_filtered, "Guid", "LinkedAssetId")
        """ rename typename column """
        atlas_rdd_df_filtered = ColumnFunctions.rename_col(atlas_rdd_df_filtered, "TypeName", "LinkedAssetType")
        atlas_rdd_df_filtered = AtlasRddColumnFunctions.add_sensitivity_label_id(atlas_rdd_df_filtered)
        label_df = atlas_rdd_df_filtered.select("LinkedAssetId", "LinkedAssetType", "LabelId")
        return label_df

    def build_collection_schema(atlas_rdd_df):
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "CollectionPath", "FriendlyPath")
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "CreateTime", "CreatedAt")
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "UpdateTime", "ModifiedAt")
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "UpdatedBy", "ModifiedBy")
        collection_schema_df = atlas_rdd_df.select("CollectionId", "FriendlyPath","CreatedAt", "ModifiedAt", "CreatedBy", "ModifiedBy")
        return collection_schema_df

    def build_data_asset_column_schemas(atlas_rdd_df):
        atlas_rdd_df = atlas_rdd_df.select("Guid", "SchemaEntities")

        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df,"Guid", "DataAssetId")

        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entities_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.explode_outer_schema_entities(atlas_rdd_df)

        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entities_attribute(atlas_rdd_df)

        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entities_relationship_attribute_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.explode_outer_classifications_from_schema_entities_col(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_data_asset_column_id(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_display_name(atlas_rdd_df, "SchemaEntityAttributes", "DisplayName")
        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entities_qualified_name(atlas_rdd_df)
        #catalog_df = catalog_df.add_classification_schema(catalog_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_classification_column_from_schema_entities(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_valid_description_column_from_schema_entities(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_valid_user_description_column_from_schema_entities(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_terms_column_from_schema_entities(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entity_classification_source(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_schema_entities_classification_rule_id(atlas_rdd_df)
        
        data_asset_column_df = atlas_rdd_df.select("DataAssetId", "DataAssetColumnId", "DisplayName", "QualifiedName", "HasValidDescription", "HasUserDescription", "HasClassification", "HasTerms",
                                                   "ClassificationId", "ClassificationSource")

        classification_association_df = atlas_rdd_df.select("ClassificationId", "ClassificationSource", "DataAssetId", "DataAssetColumnId")

        return data_asset_column_df

    def build_data_asset_schema(atlas_rdd_df):
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df,"Guid", "DataAssetId")
        atlas_rdd_df = AtlasRddColumnFunctions.add_attribute_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_display_name(atlas_rdd_df, "Attributes", "DataAssetDisplayName")
        atlas_rdd_df = AtlasRddColumnFunctions.add_description(atlas_rdd_df, "Attributes", "DataAssetDescription")
        atlas_rdd_df = AtlasRddColumnFunctions.add_data_owner(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.compute_asset_curation(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_manual_classification_column(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_scanned_classification_column(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_schema_column(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_clasification_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.explode_outer_classifications_from_classification_col(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_sensitive_classification(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_valid_description_column(atlas_rdd_df)
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "CreateTime", "CreatedAt")
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "UpdateTime", "ModifiedAt")
        atlas_rdd_df = ColumnFunctions.rename_col(atlas_rdd_df, "UpdatedBy", "ModifiedBy")
        atlas_rdd_df = AtlasRddColumnFunctions.add_qualified_name(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_sourcetype(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_platform_and_provider_columns(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_valid_owner_column(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_object_type(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.compute_has_classification(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_reason_for_unclassified(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_relationship_attribute_schema(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_has_terms_column(atlas_rdd_df)
        atlas_rdd_df = AtlasRddColumnFunctions.add_instance(atlas_rdd_df)
        
        data_asset_df = atlas_rdd_df.select("DataAssetId", "CollectionId", "CurationLevel", "DataAssetDescription", "DataAssetDisplayName", "HasGlossaryTerm", "HasManualClassification", 
                                        "HasScannedClassification", "HasSchema", "HasSensitiveClassification", "HasValidDescription", "UnclassificationReason", "SourceType", "SourceInstance",
                                        "QualifiedName", "Provider", "Platform", "ObjectType", "ModifiedBy","ModifiedAt", "CreatedBy", "CreatedAt", "HasValidOwner" )
        return data_asset_df
