from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_common_variables import AtlasRddCommonVariables
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_map import AtlasRddMap

class AtlasRddTransformations:
    
    def get_display_name(attributes_col):
        return (
            when(
                attributes_col.getField("displayName").isNotNull(),
                attributes_col.getField("displayName"))
                .when(
                attributes_col.getField("name").isNotNull(),
                attributes_col.getField("displayName"))
                .otherwise("")
        )
    
    def get_description(attributes_col):
         return (
            when(
                attributes_col.getField("description").isNotNull(),
                attributes_col.getField("description")
            ).otherwise("")
        )

    def has_description(attributes_col):
         return (
            when(
                attributes_col.getField("description").isNotNull(),
                True
            ).otherwise(False)
        )
    
    def has_user_description(attributes_col):
        return (
            when(
                attributes_col.getField("userDescription").isNotNull(),
                True
            ).otherwise(False)
        )

    def get_user_description(attributes_col):
        return (
            when(
                attributes_col.getField("userDescription").isNotNull(),
                attributes_col.getField("userDescription")
            ).otherwise("")
        )

    def get_qualified_name(attributes_col):
        return (
            when(
                attributes_col.getField("qualifiedName").isNotNull() &
                (attributes_col.getField("qualifiedName").startswith("https://app.powerbi.com") &
                 attributes_col.getField("displayName").isNotNull()),
                attributes_col.getField("displayName"))
                .otherwise("")
        )
    
    def get_classification_rule_id(classification_col):
        return when(classification_col.isNotNull() & classification_col.getField(
            "SourceDetails").isNotNull() & classification_col.getField("SourceDetails").getField(
            "MCE_RuleId").isNotNull(),
                    concat_ws(":",
                              classification_col.getField("typeName"),
                              classification_col.getField("SourceDetails").getField("MCE_RuleId"))) \
            .when(classification_col.isNotNull() & classification_col.getField("typeName").isNotNull(),
                  concat_ws(":",
                            classification_col.getField("typeName"),
                            classification_col.getField("typeName"))) \
            .otherwise(lit(""))

    def get_classification_rule_type(classification_col):
        return when(classification_col.isNotNull() & classification_col.getField(
            "SourceDetails").isNotNull() & classification_col.getField("SourceDetails").getField(
            "ClassificationRuleType").isNotNull(),
                              classification_col.getField("SourceDetails").getField("ClassificationRuleType")) \
            .otherwise(lit(""))
    
    def format_classification_type_name(classification_col):
        return lit(classification_col.getField("typeName"))

    def format_classification_category(classification_col):
        return AtlasRddTransformations.get_classification_category(
            classification_col.getField("typeName"))

    def format_classification_source(classification_col):
        return lit(classification_col.getField("source"))

    def get_classification_category(classification_col):
        """Method to get classification category from json column

        Classification: ClassificationCategory, Classification, ClassificationType
        Example:        A                       Microsoft.A.B     System
        Example:        Custom                  Random.XYZ        Custom

        Returns
        -------
        ClassificationType:ClassificationCategory
        """

        return (
            when(classification_col.isNull(), lit(""))
                .when(lower(classification_col).startswith("microsoft"),
                      initcap(split(classification_col, "\.").getItem(1)))
                .otherwise(lit(AtlasRddCommonVariables.CustomClassificationCategory))
        )
    
    def has_terms(attributes_col):
        return when(attributes_col.isNotNull() & attributes_col.getField(
            "meanings").isNotNull() & attributes_col.getField("meanings").getField(
            "terms").isNotNull(),
                            True) \
            .otherwise(False)
    
    def get_data_asset_column_id(attributes_col):
        return (
            when(
                col(attributes_col, "$.guid").isNotNull(),
                get_json_object(attributes_col, "$.guid")
            ).otherwise("")
        )
    
    def get_reason_for_unclassified(extended_props_col, classification_col):
        """
        Returns reason_for_unclassified if there is one otherwise an empty string
        The reason_for_unclassified is in the "ExtendedProps" column in one of the following fields exceptionThrown,
        samplingLabel, isLeafNode, lowConfidenceTagsFound

        Parameters
        ----------
        extended_props_col: pyspark column
            The asset attributes

        """
        return (
            when(
                (classification_col == True), lit("")
            ).when(
                get_json_object(extended_props_col, "$.isLeafNode").isNotNull()
                & get_json_object(extended_props_col, "$.isLeafNode").eqNullSafe(lit(False)),
                AtlasRddMap.reason_for_unclassified_codes["non_leaf"]
            ).when(
                get_json_object(extended_props_col, "$.samplingLabel").isNotNull()
                & get_json_object(extended_props_col, "$.samplingLabel").eqNullSafe("basicinfo"),
                AtlasRddMap.reason_for_unclassified_codes["no_selection"]
            ).when(
                get_json_object(extended_props_col, "$.lowConfidenceTagsFound").isNotNull()
                & get_json_object(extended_props_col, "$.lowConfidenceTagsFound").eqNullSafe(lit(True)),
                AtlasRddMap.reason_for_unclassified_codes["low_confidence"]
            ).when(
                get_json_object(extended_props_col, "$.exceptionThrown").isNotNull()
                & get_json_object(extended_props_col, "$.exceptionThrown").eqNullSafe(lit(True)),
                AtlasRddMap.reason_for_unclassified_codes["processing_error"]
            ).otherwise(AtlasRddMap.reason_for_unclassified_codes["no_match"])
        )

    def get_is_file(attributes_col):
        """
        Returns the True if the field isFile is in the RDD,  False otherwise
        The isFile field  should be found in the Attributes column

        Parameters
        ----------
        attributes_col: pyspark column
            The asset attributes

        """

        return (
            when(
                attributes_col.getField("isFile").isNotNull()
                & attributes_col.getField("isFile").eqNullSafe("true"),
                lit(True)
            ).when(
                attributes_col.getField("isBlob").isNotNull()
                & attributes_col.getField("isBlob").eqNullSafe("true"),
                lit(True)
            ).otherwise(lit(False))
        )

    def get_instance_udf(qualifiedname_column):
        """
         Returns the corresponding Object Type from the object type mapping and the TypeName column value
        Parameters
        ----------
        qualifiedname_column: pyspark column
            The entity type name

        Returns
        -------
        The Instance value

        """
        try:
            parsed_url = urlparse(qualifiedname_column)
            return parsed_url.netloc
        except:
            # ignore exception as this is a UDF
            return ""

    def get_data_owner(contacts_col):
        """
        Returns the Data owner if there is one otherwise an empty string
        The data owner is in the "owner" field of the Contacts column

        Parameters
        ----------
        contacts_col: pyspark column
            The asset attributes

        """
        return (
            when(
                get_json_object(contacts_col, "$.Owner").isNotNull(),
                get_json_object(contacts_col, "$.Owner")
            ).otherwise(None)
        )

    def get_platform(source_type_col: Column, source_platform_mapping: MapType):
        return when((source_type_col.isNull() | source_type_col.eqNullSafe(lit(""))), lit("")) \
            .when(source_platform_mapping[source_type_col].isNotNull(), source_platform_mapping[source_type_col]) \
            .otherwise(lit('Unknown'))

    def get_provider(source_type_col: Column, source_provider_mapping: MapType):
        return when((source_type_col.isNull() | source_type_col.eqNullSafe(lit(""))), lit("")) \
            .when(source_provider_mapping[source_type_col].isNotNull(), source_provider_mapping[source_type_col]) \
            .otherwise(lit('Others'))

    def has_classification_rule_type(classification_col):
        return when((classification_col.isNotNull()) & 
                    (classification_col.getField("SourceDetails").isNotNull()) & 
                    (classification_col.getField("SourceDetails").getField("ClassificationRuleType").isNotNull()) & 
                    (classification_col.getField("SourceDetails").getField("ClassificationRuleType") == "System"),
                            True) \
            .otherwise(False)

    def calculate_curation_number(
            atlasrdd_df: DataFrame):

        added_tag_type = atlasrdd_df \
            .withColumn("CurationLevel",
                        lit(0))

        for curr_column in AtlasRddMap.curation_def_columns:
            added_tag_type = added_tag_type \
                .withColumn("CurationLevel",
                            when(~(col(curr_column).eqNullSafe(lit(""))), (col("CurationLevel") + lit(1)))
                            .otherwise(col("CurationLevel")))

        return added_tag_type

    def get_curation_level(curation_column, list_len):
        return (
        when(curation_column.eqNullSafe(lit(list_len)), "FullyCurated")
            .when(curation_column.eqNullSafe(lit(0)), "NotCurated")
            .otherwise("PartiallyCurated")
        )
    
    def get_terms(attributes_col):
        return when(attributes_col.isNotNull() & attributes_col.getField(
            "meanings").isNotNull() & attributes_col.getField("meanings").getField(
            "terms").isNotNull(),
                            True) \
            .otherwise(False)
