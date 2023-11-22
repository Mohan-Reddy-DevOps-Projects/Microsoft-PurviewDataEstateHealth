import uuid
from pyspark.sql.functions import *
import datetime
import pyspark.sql.functions as f

class BusinessDomainTrendTransformations:

    def calculate_has_valid_dp_owner_pass_count(dataproduct_df):
        has_valid_dp_owner_pass_count_added = dataproduct_df.withColumn(
            "ValidDataProductOwnerPassCount", lit(col("HasValidOwner"))
            )
        return has_valid_dp_owner_pass_count_added
    
    def calculate_authoritative_source_pass_count(dataproduct_df):
        authoritative_source_pass_count_added = dataproduct_df.withColumn(
            "AuthoritativeSourcePassCount", lit(col("IsAuthoritativeSource"))
            )                                                              
        return authoritative_source_pass_count_added

    def calculate_classification_pass_count(dataproduct_df):
        classification_pass_count_added = dataproduct_df.withColumn(
            "ClassificationPassCount", when(col("ClassificationPassCount") > 0, lit(1))
            .otherwise(lit(0))
            ) 
        return classification_pass_count_added
    
    
    def calculate_access_entitlement_pass_count(dataproduct_df):
         access_entitlement_pass_count_added = dataproduct_df.withColumn(
            "AccessEntitlementPassCount", lit(col("HasAccessEntitlement"))
            )
         return access_entitlement_pass_count_added

    def calculate_data_share_aggrement_set_or_exempt_pass_count(dataproduct_df):
        data_share_aggrement_set_or_exempt_pass_count_added = dataproduct_df.withColumn(
            "DataShareAgreementSetOrExemptPassCount", lit(col("HasDataShareAgreementSetOrExempt"))
            )
        return data_share_aggrement_set_or_exempt_pass_count_added
    
    def calculate_data_qality_score_pass_count(dataproduct_df):
        data_qality_score_pass_count_added = dataproduct_df.withColumn(
            "DataQualityScorePassCount", lit(col("HasDataQualityScore"))
            )
        return data_qality_score_pass_count_added
    
    def calculate_valid_terms_of_use_pass_count(dataproduct_df):
        valid_terms_of_use_pass_count_added = dataproduct_df.withColumn(
            "ValidTermsOfUsePassCount", lit(col("HasValidTermsOfUse"))
            )
        return valid_terms_of_use_pass_count_added
    
    def calculate_valid_use_case_pass_count(dataproduct_df):
        valid_use_case_pass_count_added = dataproduct_df.withColumn(
            "ValidUseCasePassCount", lit(col("HasValidUseCase"))
            )
        return valid_use_case_pass_count_added
    
    def calculate_dataproduct_description_pass_count(dataproduct_df):
        dataproduct_description_pass_count_added = dataproduct_df.withColumn(
            "DataProductDescriptionPassCount", lit(col("HasDescription"))
            )
        return dataproduct_description_pass_count_added
    
    def calculate_glossary_term_pass_count(dataproduct_df):
        glossary_term_pass_count_added = dataproduct_df.withColumn(
            "GlossaryTermPassCount",  when(col("GlossaryTermCount")>0, 1)
            .otherwise(0)
            )
        return glossary_term_pass_count_added
    
    def calculate_sum_for_all_columns(dataproduct_df):
        dataproduct_df = dataproduct_df.groupBy("BusinessDomainId").sum()
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(AccessEntitlementPassCount)","AccessEntitlementPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidTermsOfUsePassCount)","ValidTermsOfUsePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidUseCasePassCount)","ValidUseCasePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(AuthoritativeSourcePassCount)","AuthoritativeSourcePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ClassificationPassCount)","ClassificationPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataShareAgreementSetOrExemptPassCount)","DataShareAgreementSetOrExemptPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidDataProductOwnerPassCount)","ValidDataProductOwnerPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataProductDescriptionPassCount)","DataProductDescriptionPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataQualityScorePassCount)","DataQualityScorePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(GlossaryTermPassCount)","GlossaryTermPassCount")
        return dataproduct_df
    
    def calculate_aggregated_sum_for_all_columns(businessdomain_trend_df):
        businessdomain_trend_df = businessdomain_trend_df.groupBy().sum()
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(DataProductCount)","DataProductCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(AssetCount)","AssetCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(TotalOpenActionsCount)","TotalOpenActionsCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(AccessEntitlementPassCount)","AccessEntitlementPassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(ValidTermsOfUsePassCount)","ValidTermsOfUsePassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(ValidUseCasePassCount)","ValidUseCasePassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(AuthoritativeSourcePassCount)","AuthoritativeSourcePassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(ClassificationPassCount)","ClassificationPassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(DataShareAgreementSetOrExemptPassCount)","DataShareAgreementSetOrExemptPassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(ValidDataProductOwnerPassCount)","ValidDataProductOwnerPassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(DataProductDescriptionPassCount)","DataProductDescriptionPassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(DataQualityScorePassCount)","DataQualityScorePassCount")
        businessdomain_trend_df = businessdomain_trend_df.withColumnRenamed("sum(GlossaryTermPassCount)","GlossaryTermPassCount")
        return businessdomain_trend_df
    
    def calculate_businessdomain_count(businessdomain_trend_df, count):
        businessdomain_count_added = businessdomain_trend_df.withColumn(
            "BusinessDomainCount",  lit(count)
            )
        return businessdomain_count_added
