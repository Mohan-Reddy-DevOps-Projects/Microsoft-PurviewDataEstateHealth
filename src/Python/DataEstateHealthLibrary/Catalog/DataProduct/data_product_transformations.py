from pyspark.sql.functions import *
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

class DataProductTransformations:
    def calculate_is_primary_dataproduct(dataproduct_df):
        is_primary_dataproduct_added = dataproduct_df.withColumn(
            "IsPrimaryDataProduct", lit(True)
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
            "IsAuthoritativeSource", lit(False)
        )

        return is_authoritative_source_added

    def calculate_has_data_quality_score(dataproduct_df):
        has_data_quality_score_added = dataproduct_df.withColumn(
            "HasDataQualityScore", lit(False)
        )

        return has_data_quality_score_added

    def calculate_has_data_share_agreement_set_or_exempt(dataproduct_df):
        has_data_share_agreement_set_or_exempt_added = dataproduct_df.withColumn(
            "HasDataShareAgreementSetOrExempt", lit(False)
        )

        return has_data_share_agreement_set_or_exempt_added
    
    def calculate_has_description(dataproduct_df):
        has_description_added = dataproduct_df.withColumn(
            "HasDescription", when(col("Description").isNotNull(), True)
            .otherwise(False)
        )

        return has_description_added
    
    def calculate_has_valid_owner(dataproduct_df):
        has_valid_owner_added = dataproduct_df.withColumn(
            "HasValidOwner", when(col("Contacts").getField("owner").isNotNull(), True)
            .otherwise(False)
        )

        return has_valid_owner_added

    def calculate_has_valid_terms_of_use(dataproduct_df):
        has_valid_terms_of_use_added = dataproduct_df.withColumn(
            "HasValidTermsOfUse", when(col("TermsOfUse").isNotNull(), True)
            .otherwise(False)
        )

        return has_valid_terms_of_use_added

    def calculate_has_valid_use_case(dataproduct_df):
        has_valid_use_case_added = dataproduct_df.withColumn(
            "HasValidUseCase", when(col("BusinessUse").isNotNull() &
                (~(col("BusinessUse")=="")), True)
            .otherwise(False)
        )

        return has_valid_use_case_added

