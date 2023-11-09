from pyspark.sql.functions import *
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

class DataProductTransformations:
    def calculate_is_primary_dataproduct(dataproduct_df):
        is_primary_dataproduct_added = dataproduct_df.withColumn(
            "IsPrimaryDataProduct", lit(1)
        )

        return is_primary_dataproduct_added

    def calculate_data_product_count_for_domain(dataproduct_domain_association_df):
        dataproduct_count_added = dataproduct_domain_association_df.groupBy("BusinessDomainId").count()
        dataproduct_count_added = ColumnFunctions.rename_col(dataproduct_count_added, "count", "DataProductsCount")

        return dataproduct_count_added

    def calculate_glossary_term_count(dataproduct_df):
        glossary_term_count_added = dataproduct_df.withColumn(
            "GlossaryTermCount", lit(0)
        )

        return glossary_term_count_added

    def calculate_is_authoritative_source(dataproduct_df):
        is_authoritative_source_added = dataproduct_df.withColumn(
            "IsAuthoritativeSource", lit(0)
        )

        return is_authoritative_source_added

    def calculate_has_data_quality_score(dataproduct_df):
        has_data_quality_score_added = dataproduct_df.withColumn(
            "HasDataQualityScore", lit(0)
        )

        return has_data_quality_score_added

    def calculate_has_data_share_agreement_set_or_exempt(dataproduct_df):
        has_data_share_agreement_set_or_exempt_added = dataproduct_df.withColumn(
            "HasDataShareAgreementSetOrExempt", lit(0)
        )

        return has_data_share_agreement_set_or_exempt_added
    
    def calculate_has_access_entitlement(dataproduct_df):
        has_access_entitlement_added = dataproduct_df.withColumn(
            "HasAccessEntitlement", lit(0)
        )

        return has_access_entitlement_added

    def calculate_classification_pass_count(dataproduct_df):
        classification_pass_count_added = dataproduct_df.withColumn(
            "ClassificationPassCount", lit(0)
        )

        return classification_pass_count_added

    def calculate_has_not_null_description(dataproduct_df):
        has_not_null_description_added = dataproduct_df.withColumn(
            "HasNotNullDescription", when(col("Description").isNotNull(), 1)
            .otherwise(0)
        )

        return has_not_null_description_added
    
    def calculate_has_valid_owner(dataproduct_df):
        has_valid_owner_added = dataproduct_df.withColumn(
            "HasValidOwner", when(col("Contacts").getField("owner").isNotNull(), 1)
            .otherwise(0)
        )

        return has_valid_owner_added

    def calculate_has_valid_terms_of_use(dataproduct_df):
        has_valid_terms_of_use_added = dataproduct_df.withColumn(
            "HasValidTermsofUse", when(col("TermsOfUse").isNotNull(), 1)
            .otherwise(0)
        )

        return has_valid_terms_of_use_added

    def calculate_has_valid_use_case(dataproduct_df):
        has_valid_use_case_added = dataproduct_df.withColumn(
            "HasValidUseCase", when(col("BusinessUse").isNotNull() &
                (~(col("BusinessUse")=="")), 1)
            .otherwise(0)
        )

        return has_valid_use_case_added
    
    def add_asset_count(dataproduct_df):
        asset_count_added = dataproduct_df.withColumn(
            "AssetCount", get_json_object(col("AdditionalProperties"),"$.assetCount"
        ))

        return asset_count_added

