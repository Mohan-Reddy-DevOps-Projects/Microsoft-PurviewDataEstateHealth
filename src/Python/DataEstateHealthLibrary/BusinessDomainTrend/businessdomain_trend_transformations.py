import uuid
from pyspark.sql.functions import *
import datetime
import pyspark.sql.functions as f

class BusinessDomainTrendTransformations:
        
    def calculate_business_domain_trend_id(dataproduct_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        uuid_udf = f.udf(lambda : str(uuid.uuid4()), StringType())
        trend_id_added = dataproduct_df.withColumn(
            "BusinessDomainTrendId", uuid_udf()
            )
        
        return trend_id_added

    def calculate_has_valid_dp_owner_pass_count(dataproduct_df):
        has_valid_dp_owner_pass_count_added = dataproduct_df.withColumn(
            "ValidDataProductOwnerPassCount", lit(col("HasValidOwner"))
            )
        return has_valid_dp_owner_pass_count_added
    
    def calculate_authoritative_source_pass_count(dataproduct_df):
        authoritative_source_pass_count_added = dataproduct_df.withColumn(
            "AuthoratativeSourcePassCount", lit(col("IsAuthoritativeSource"))
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
            "ValidTermsOfUsePassCount", lit(col("HasValidTermsofUse"))
            )
        return valid_terms_of_use_pass_count_added
    
    def calculate_valid_use_case_pass_count(dataproduct_df):
        valid_use_case_pass_count_added = dataproduct_df.withColumn(
            "ValidUseCasePassCount", lit(col("HasValidUseCase"))
            )
        return valid_use_case_pass_count_added
    
    def calculate_not_null_description_pass_count(dataproduct_df):
        not_null_description_pass_count_added = dataproduct_df.withColumn(
            "NotNullDataProductDescriptionPassCount", lit(col("HasNotNullDescription"))
            )
        return not_null_description_pass_count_added
    
    def calculate_glossary_term_pass_count(dataproduct_df):
        glossary_term_pass_count_added = dataproduct_df.withColumn(
            "GlossaryTermPassCount",  when(col("GlossaryTermCount")>0, 1)
            .otherwise(0)
            )
        return glossary_term_pass_count_added
