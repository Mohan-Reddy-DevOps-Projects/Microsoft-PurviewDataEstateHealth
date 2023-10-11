from pyspark.sql.functions import *
from pyspark.sql.types import *
from itertools import chain
from urllib.parse import urlparse
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_schemas import AtlasRddScehmas
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_common_variables import AtlasRddCommonVariables
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_transformations import AtlasRddTransformations
from DataEstateHealthLibrary.AtlasRdd.atlas_rdd_map import AtlasRddMap
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class AtlasRddColumnFunctions:
    
    def add_clasification_schema(atlasrdd_df):
        """Add schema to columns"""
        added_classification = atlasrdd_df.withColumn(
            "Classifications",
            from_json(col("Classifications"), AtlasRddScehmas.classification_json_schema
            )
        )

        return added_classification
    
    def add_classification_attribute_schema(atlasrdd_df):
        """Add schema to columns"""
        added_attributes = atlasrdd_df.withColumn(
            "ClassificationAttributes", col("Classification").getField("attributes")
        )

        return added_attributes

    def add_display_name(atlasrdd_df,colName, newColName):
        return atlasrdd_df.withColumn(
            newColName, AtlasRddTransformations.get_display_name(col(colName))
        )
    
    def add_description(atlasrdd_df, colName, newColName):
        return atlasrdd_df.withColumn(
            newColName, AtlasRddTransformations.get_description(col(colName))
        )
    
    def add_classification_rule_id(atlasrdd_df):
        return atlasrdd_df\
            .withColumn("ClassificationId",
                        AtlasRddTransformations.get_classification_rule_id(col("Classification")))
    
    def add_schema_entities_classification_rule_id(atlasrdd_df):
        return atlasrdd_df\
            .withColumn("ClassificationId",
                        AtlasRddTransformations.get_classification_rule_id(col("EntityClassification")))
    
    def add_classification_rule_type(atlasrdd_df):
        return atlasrdd_df\
            .withColumn("ClassificationType",
                        AtlasRddTransformations.get_classification_rule_type(col("Classification")))

    def explode_outer_classifications_from_classification_col(atlasrdd_df):
       
        return atlasrdd_df.withColumn(
            "Classification", explode_outer(atlasrdd_df.Classifications)
        )
    
    def add_schema_entity_classification_source(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "ClassificationSource", AtlasRddTransformations.format_classification_source(col("EntityClassification"))
        )

    def compute_classification(atlasrdd_df):
        atlasrdd_df = AtlasRddColumnFunctions.add_clasification_schema(atlasrdd_df)

        atlasrdd_df = AtlasRddColumnFunctions.explode_outer_classifications_from_classification_col(atlasrdd_df)

        format_classification = atlasrdd_df \
            .withColumn("Name",
                        when(col("Classification").isNull(), lit(""))
                        .otherwise(AtlasRddTransformations.format_classification_type_name(
                            col("Classification")))
                        ) \
            .withColumn("ClassificationSource",
                        when(col("Classification").isNull(), lit(""))
                        .otherwise(AtlasRddTransformations.format_classification_source(
                            col("Classification")))
                        )

        format_classification_category = format_classification.withColumn(
            "Category",
            when(col("Classification").isNull(), lit("")
                 ).otherwise(
                AtlasRddTransformations.get_classification_category(col("Classification").getField("typeName"))
            )
        )
        return format_classification_category

    def add_sensitivity_label_id(atlasrdd_df):
        """ get_json_object is used to extract the JSON string based on path from the JSON column.  
        here we fetch id from sensitivity label json string and add it as a new column label id"""
        #AtassRddLabelDF = fileteredDF.select("Guid", "TypeName", get_json_object(col("SensitivityLabel"),"$.id").alias("LabelId"))
        return atlasrdd_df.withColumn(
            "LabelId", get_json_object(col("SensitivityLabel"),"$.id")
        )

    def add_has_valid_user_description_column_from_schema_entities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "HasUserDescription", AtlasRddTransformations.has_user_description(col("SchemaEntityAttributes"))
        )
    
    def add_has_valid_description_column_from_schema_entities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "HasValidDescription", AtlasRddTransformations.has_description(col("SchemaEntityAttributes"))
        )
    
    def add_has_terms_column_from_schema_entities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "HasTerms", when(
                atlasrdd_df.SchemaEntityRelationshipAttributes.isNotNull() &
                AtlasRddTransformations.has_terms(col("SchemaEntityRelationshipAttributes")),
                True
            ).otherwise(False)
        )
    
    def add_has_classification_column_from_schema_entities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "HasClassification", when(
                atlasrdd_df.EntityClassification.isNotNull(),
                True
            ).otherwise(False)
        )
    
    def add_schema_entities_classification(atlasrdd_df):
        added_classification = atlasrdd_df.withColumn(
            "SchemaEntitiesClassification", col("SchemaEntity").getField("classifications")
        )

        return added_classification

    def explode_outer_schema_entities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "SchemaEntity", explode_outer(atlasrdd_df.SchemaEntities)
        )
    
    def add_schema_entities_schema(atlasrdd_df):
        """Add schema to columns"""
        added_classification = atlasrdd_df.withColumn(
            "SchemaEntities", from_json(col("SchemaEntities"), AtlasRddScehmas.schema_entities_json_schema)
        )

        return added_classification

    def add_schema_entities_attribute(atlasrdd_df):
        added_attributes = atlasrdd_df.withColumn(
            "SchemaEntityAttributes", col("SchemaEntity").getField("attributes")
        )

        return added_attributes

    def explode_outer_schemaEntities(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "SchemaEntity", explode_outer(atlasrdd_df.SchemaEntities)
        )
    
    def add_schema_entities_relationship_attribute_schema(atlasrdd_df):
        """Add schema to columns"""
        added_relationshipAttributes = atlasrdd_df.withColumn(
            "SchemaEntityRelationshipAttributes", col("SchemaEntity").getField("relationshipAttributes")
        )

        return added_relationshipAttributes

    def add_relationship_attribute_schema(atlasrdd_df):
        """Add schema to columns"""
        added_relationshipAttributes = atlasrdd_df.withColumn(
            "RelationshipAttributes", from_json(col("RelationshipAttributes"), AtlasRddScehmas.relationship_attributes_json_schema)
        )

        return added_relationshipAttributes

    def add_has_terms_column(atlasrdd_df):
       return atlasrdd_df.withColumn(
            "HasGlossaryTerm", when(
                atlasrdd_df.RelationshipAttributes.isNotNull() &
                AtlasRddTransformations.get_terms(atlasrdd_df.RelationshipAttributes),
                True
            ).otherwise(False)
        )
    
    def explode_outer_classifications_from_schema_entities_col(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "EntityClassification", explode_outer(col("SchemaEntity").getField("classifications"))
        )
    
    def add_schema_entities_qualified_name(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "QualifiedName", AtlasRddTransformations.get_qualified_name(col("SchemaEntityAttributes"))
        )
    
    def add_data_asset_column_id(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "DataAssetColumnId", col("SchemaEntity").getField("guid")
        )
    
    def add_reason_for_unclassified(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "UnclassificationReason",
            AtlasRddTransformations.get_reason_for_unclassified(col("ExtendedProps"), col("HasClassification"))
        )

    def add_object_type(atlasrdd_df):
        """Adds Object_type column to the dataframe

        Values are extracted using column - TypeName and the object_type_mapping

        NOTE:
         - The object type mapping was taken from the search team

        Parameters
        ----------
        atlasrdd_df - pyspark dataframe
            Catalog dataframe derived from the AtlasRDD job. Schema must contain TypeName.
        object_type_map - pyspark MapType()
        Returns
        -------
        pyspark dataframe with a 'ObjectType' column

        """
        if not atlasrdd_df:
            return atlasrdd_df

        object_type_mapping = create_map([lit(x) for x in chain(*AtlasRddMap.ObjectTypeMapping.items())])

        return atlasrdd_df.withColumn("ObjectType",
            when(
                (object_type_mapping[col("TypeName")].isNotNull())
                & (~object_type_mapping[col("TypeName")].eqNullSafe("FolderOrFile")),
                (object_type_mapping.getField(col("TypeName")))
            ).when(
                (object_type_mapping[col("TypeName")].isNotNull())
                & (object_type_mapping[col("TypeName")].eqNullSafe("FolderOrFile"))
                & (AtlasRddTransformations.get_is_file(col("Attributes")).eqNullSafe(lit(True))),
                "Files"
            ).when(
                (object_type_mapping[col("TypeName")].isNotNull())
                & (object_type_mapping[col("TypeName")].eqNullSafe("FolderOrFile"))
                & (AtlasRddTransformations.get_is_file(col("Attributes")).eqNullSafe(lit(False))),
                "Folders"
            ).otherwise("Unknown"))

    def add_instance(atlasrdd_df):
        """Adds Instance column to the dataframe

        Information is extracted using column - QualifiedName

          Example: http://abcd.com/data.txt --> abcd.com
          Example: mssql://instance/server --> instance


        Parameters
        ----------
        atlasrdd_df - pyspark dataframe
            Catalog dataframe derived from the AtlasRDD job. Schema must contain QualifiedName.
        Returns
        -------
        pyspark dataframe with a 'Instance' column

        """
        if not atlasrdd_df:
            return None

        with_formatted_fqn = AtlasRddColumnFunctions.format_qualified_name(atlasrdd_df)
        get_instance = udf(AtlasRddTransformations.get_instance_udf, StringType())

        return with_formatted_fqn\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_power_bi,

                                       ""))\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_postgres_sql,
                                       AtlasRddCommonVariables.regex_postgres_sql_replacement))\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_adf,
                                       AtlasRddCommonVariables.regex_adf_replacement))\
            .withColumn("SourceInstance", get_instance("FormattedFQN"))\
            .drop("FormattedFQN")

    def format_qualified_name(atlas_rdd_df: DataFrame) -> DataFrame:
        """
        Transform the qualified name in a format where it can be linked to the instance it belongs to (root parent) 
        qualified name.
        Example: 
            hive assets -> child: "hmstestdb.hms_external_table@https://hmsdatascantest-ssh.azurehdinsight.net"
            hive assets -> child after transformation: "zhttps://hmsdatascantest-ssh.azurehdinsight.net"
            hive assets -> parent: "https://hmsdatascantest-ssh.azurehdinsight.net"
            synapse asset-> parent: "https://synapseworksapces1rg1euap.azuresynapse.net"
            synapse asset-> child: "mssql://synapseworksapces1rg1euap.sql.azuresynapse.net/testDB1"
            synapse asset-> child after transformation: "https://synapseworksapces1rg1euap.azuresynapse.net/testDB1"
            azure blob asset-> parent: "https://blobdatascancus.core.windows.net"
            azure blob asset-> child: "https://blobdatascancus.table.core.windows.net/employeedetail"
            azure blob asset-> child after transformation: "https://blobdatascancus.core.windows.net/employeedetail"
        """
        if not atlas_rdd_df:
            return None

        return atlas_rdd_df.withColumn("FormattedFQN", col("QualifiedName"))\
            .withColumn("FormattedFQN",
                        when(col("FormattedFQN").startswith("mssql") & col("FormattedFQN").contains("azuresynapse"),  # azure synapse mssql replace the instance 'http' with 'mssql'
                             regexp_replace("FormattedFQN", "mssql", 'https')).otherwise(col("FormattedFQN")))\
            .withColumn("FormattedFQN",
                        when((col("FormattedFQN").contains("@") & col("TypeName").contains("hive")),  # hive fqn db.table@instance
                             array_join(reverse(split(col("FormattedFQN"), '@')), delimiter="/")).otherwise(col("FormattedFQN")))\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_adls, # azure delta lake appends a '.file or .table or .dfs' to the fqn of the instance
                                       AtlasRddCommonVariables.regex_adls_replacement))\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_synapse_fqn,
                                       AtlasRddCommonVariables.regex_synapse_fqn_replacement))\
            .withColumn("FormattedFQN",
                        regexp_replace("FormattedFQN",
                                       AtlasRddCommonVariables.regex_synapse_ondemand_fqn,
                                       AtlasRddCommonVariables.regex_synapse_ondemand_fqn_replacement))

    def add_has_valid_owner_column(atlasrdd_df):
       
        return atlasrdd_df.withColumn(
            "HasValidOwner", when(
                (atlasrdd_df.DataOwner.isNotNull()) &
                (atlasrdd_df.DataOwner != ""),
                True
            ).otherwise(False)
        )
    
    def add_data_owner(atlasrdd_df):
        """Adds DataOwner column to the dataframe
        The data owner is extracted from using column -  Attributes
        Parameters
        ----------
        atlasrdd_df - pyspark dataframe
            Catalog dataframe derived from the AtlasRDD job. Schema must contain column-Attributes.
        Returns
        -------
        pyspark dataframe with a 'DataOwner' column
        """
        exploded_owner = atlasrdd_df.withColumn(
            "DataOwners", explode_outer(
                from_json(
                    AtlasRddTransformations.get_data_owner(col("Contacts")),
                    AtlasRddScehmas.data_owner_json_schema)
            )
        ).withColumn(
            "DataOwner",
            when(col("DataOwners").isNotNull(), col("DataOwners").id)
                .otherwise(lit(""))
        ).drop("DataOwners")

        return exploded_owner

    def add_platform_and_provider_columns(atlasrdd_df):
        """
        Adds platform and provider columns to RDD using insights config file
        :param insight_config_path: path to insights_config file
        :param rdd: DataFrame
        :param output_column_names: filter columns for output
        :return: Dataframe with provider and platform columns
        """
        source_platform_provider_mapping = HelperFunction.create_source_platform_provider_mapping()

        return AtlasRddColumnFunctions.add_platform_provider(atlasrdd_df, source_platform_provider_mapping)

    def add_platform_provider(atlasrdd_df, source_provider_mapping):
        return atlasrdd_df \
            .withColumn("Provider", AtlasRddTransformations.get_provider(atlasrdd_df.SourceType, source_provider_mapping[0])) \
            .withColumn("Platform", AtlasRddTransformations.get_platform(atlasrdd_df.SourceType, source_provider_mapping[1]))

    def add_sourcetype(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "SourceType",
            when(col("TypeName").isin(AtlasRddMap.TypeNameFileList), lit("FileBased")).otherwise(lit("TableBased")),
        )

    def add_qualified_name(atlasrdd_df):
        return atlasrdd_df.withColumn(
            "QualifiedName", AtlasRddTransformations.get_qualified_name(col("Attributes"))
        )

    def add_has_valid_description_column(atlasrdd_df):

        return atlasrdd_df.withColumn(
            "HasValidDescription", AtlasRddTransformations.has_description(col("Attributes"))
        )

    def add_has_sensitive_classification(atlasrdd_df):
        return atlasrdd_df\
            .withColumn("HasSensitiveClassification",
                        when(atlasrdd_df.Classifications.isNotNull(),
                             AtlasRddTransformations.has_classification_rule_type(col("Classification")))
            )

    def add_attribute_schema(atlasrdd_df):
        """Add schema to columns"""
        added_attributes = atlasrdd_df.withColumn(
            "Attributes", from_json(col("Attributes"), AtlasRddScehmas.attributes_json_schema)
        )

        return added_attributes

    def compute_asset_curation(atlasrdd_df):
        if atlasrdd_df:
            added_tag = AtlasRddColumnFunctions.add_curation_level(atlasrdd_df)
            return added_tag
        return None

    def add_curation_level(
            atlasrdd_df: DataFrame):
        added_tag_type = AtlasRddTransformations.calculate_curation_number(atlasrdd_df)

        curation_level_df = added_tag_type.withColumn("CurationLevel",
                                                      AtlasRddTransformations.get_curation_level(col("CurationLevel"),len(AtlasRddMap.curation_def_columns)))
        return curation_level_df

    def add_has_manual_classification_column(atlasrdd_df):
       return atlasrdd_df.withColumn("HasManualClassification",
                                    when(~(atlasrdd_df.Source == "DataScan") &
                                         atlasrdd_df.Classifications.isNotNull(), True)
                                         .otherwise(False))

    def add_has_scanned_classification_column(atlasrdd_df):
       return atlasrdd_df.withColumn("HasScannedClassification",
                                    when((atlasrdd_df.Source == "DataScan") &
                                         atlasrdd_df.Classifications.isNotNull(), True)
                                         .otherwise(False))

    def add_has_schema_column(atlasrdd_df):
       return atlasrdd_df.withColumn("HasSchema",
                                    when(atlasrdd_df.SchemaEntities.isNotNull(), True)
                                         .otherwise(False))

    def compute_has_classification(catalog_df):
       df = catalog_df.withColumn("HasClassification",when(catalog_df.Classifications.isNotNull(), True)
                          .otherwise(False)
       )           
       return df
