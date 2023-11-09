from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from functools import reduce
from pyspark.sql import DataFrame
from pyspark.sql.types import *
from DataEstateHealthLibrary.BusinessDomainTrend.businessdomain_trend_transformations import BusinessDomainTrendTransformations
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class BusinessDomainTrendAggregation:
    def aggregate_business_domain_trend(businessdomain_df, dataproduct_df, productdomain_association_df, assetdomain_association_df):
        
        #join to get associated domain id for data product
        dataproduct_df = productdomain_association_df.join(dataproduct_df,"DataProductId","leftouter")
        dataproduct_df = BusinessDomainTrendTransformations.calculate_authoritative_source_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_has_valid_dp_owner_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_classification_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_access_entitlement_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_data_share_aggrement_set_or_exempt_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_data_qality_score_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_valid_terms_of_use_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_valid_use_case_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_not_null_description_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_glossary_term_pass_count(dataproduct_df)
        dataproduct_df = dataproduct_df.select("BusinessDomainId","AccessEntitlementPassCount","ValidTermsOfUsePassCount", "ValidUseCasePassCount","AuthoratativeSourcePassCount",
                                              "DataShareAgreementSetOrExemptPassCount","ValidDataProductOwnerPassCount","NotNullDataProductDescriptionPassCount","ClassificationPassCount",
                                             "DataQualityScorePassCount","GlossaryTermPassCount")
        
        #group rows with same business domain id into one - Bucketing
        dataproduct_df = dataproduct_df.groupBy("BusinessDomainId").sum()
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(AccessEntitlementPassCount)","AccessEntitlementPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidTermsOfUsePassCount)","ValidTermsOfUsePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidUseCasePassCount)","ValidUseCasePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(AuthoratativeSourcePassCount)","AuthoratativeSourcePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ClassificationPassCount)","ClassificationPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataShareAgreementSetOrExemptPassCount)","DataShareAgreementSetOrExemptPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(ValidDataProductOwnerPassCount)","ValidDataProductOwnerPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(NotNullDataProductDescriptionPassCount)","NotNullDataProductDescriptionPassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataQualityScorePassCount)","DataQualityScorePassCount")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(GlossaryTermPassCount)","GlossaryTermPassCount") 

        businessdomain_asset_count = CatalogColumnFunctions.calculate_count_for_domain(assetdomain_association_df, "AssetCount")
        businessdomain_asset_count = businessdomain_asset_count.select("BusinessDomainId","AssetCount")
        businessdomain_df = businessdomain_df.join(businessdomain_asset_count,"BusinessDomainId","leftouter")
        businessdomain_df = businessdomain_df.select("BusinessDomainId","BusinessDomainDisplayName", "DataProductsCount","AssetCount")
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "DataProductsCount", "DataProductCount")
        businessdomain_df = businessdomain_df.distinct()

        businessdomain_df = HelperFunction.calculate_last_refreshed_at(businessdomain_df)
        businessdomain_df = BusinessDomainTrendTransformations.calculate_business_domain_trend_id(businessdomain_df)

        businessdomain_trend_df = businessdomain_df.join(dataproduct_df,"BusinessDomainId","leftouter")
        
        businessdomain_trend_df = businessdomain_trend_df.select("BusinessDomainTrendId","BusinessDomainId","DataProductCount","AssetCount","ClassificationPassCount",
                                                                 "GlossaryTermPassCount","DataQualityScorePassCount","NotNullDataProductDescriptionPassCount","ValidDataProductOwnerPassCount",
                                                                 "DataShareAgreementSetOrExemptPassCount","AuthoratativeSourcePassCount","ValidUseCasePassCount","ValidTermsOfUsePassCount",
                                                                 "AccessEntitlementPassCount","LastRefreshedAt")
        businessdomain_trend_df = businessdomain_trend_df.na.fill(value=0)
        
        return businessdomain_trend_df
