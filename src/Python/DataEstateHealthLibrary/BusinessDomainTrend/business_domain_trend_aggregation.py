from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from functools import reduce
from pyspark.sql import DataFrame
from pyspark.sql.functions import *
from DataEstateHealthLibrary.BusinessDomainTrend.businessdomain_trend_transformations import BusinessDomainTrendTransformations
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.shared_transformations import SharedTransformations


class BusinessDomainTrendAggregation:
    def aggregate_business_domain_trends_by_id(businessdomain_df, dataproduct_df, productdomain_association_df, assetproduct_association_df, healthaction_df):
        
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
        dataproduct_df = BusinessDomainTrendTransformations.calculate_dataproduct_description_pass_count(dataproduct_df)
        dataproduct_df = BusinessDomainTrendTransformations.calculate_glossary_term_pass_count(dataproduct_df)
        dataproduct_df = dataproduct_df.select("BusinessDomainId","AccessEntitlementPassCount","ValidTermsOfUsePassCount", "ValidUseCasePassCount","AuthoritativeSourcePassCount",
                                              "DataShareAgreementSetOrExemptPassCount","ValidDataProductOwnerPassCount","DataProductDescriptionPassCount","ClassificationPassCount",
                                             "DataQualityScorePassCount","GlossaryTermPassCount")
        
        #group rows with same business domain id into one - Bucketing
        dataproduct_df = BusinessDomainTrendTransformations.calculate_sum_for_all_columns(dataproduct_df)

        if assetproduct_association_df.isEmpty() or productdomain_association_df.isEmpty():
            businessdomain_df = HelperFunction.calculate_default_column_value(businessdomain_df,"AssetCount",0) 
        else:
            businessdomain_df = SharedTransformations.calculate_asset_count(businessdomain_df, productdomain_association_df ,assetproduct_association_df,"AssetCount")
            
        if not healthaction_df.isEmpty():
            #get action count
            healthaction_df = healthaction_df.select("ActionId","BusinessDomainId")
            healthaction_df = CatalogColumnFunctions.calculate_count_for_domain(healthaction_df,"TotalOpenActionsCount")
            businessdomain_df = businessdomain_df.join(healthaction_df,"BusinessDomainId","leftouter")
        else:
            businessdomain_df = HelperFunction.calculate_default_column_value(businessdomain_df,"TotalOpenActionsCount",0)
        
        businessdomain_df = businessdomain_df.select("BusinessDomainId","BusinessDomainDisplayName", "DataProductsCount","AssetCount", "TotalOpenActionsCount")
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "DataProductsCount", "DataProductCount")
        businessdomain_df = businessdomain_df.distinct()

        businessdomain_df = HelperFunction.calculate_last_refreshed_at(businessdomain_df,"LastRefreshedAt")
        businessdomain_df = HelperFunction.calculate_uuid_column(businessdomain_df,"BusinessDomainTrendId")

        businessdomain_trend_df = businessdomain_df.join(dataproduct_df,"BusinessDomainId","leftouter")
        
        businessdomain_trend_df = businessdomain_trend_df.select("BusinessDomainTrendId","BusinessDomainId","DataProductCount","AssetCount","TotalOpenActionsCount","ClassificationPassCount",
                                                                 "GlossaryTermPassCount","DataQualityScorePassCount","DataProductDescriptionPassCount","ValidDataProductOwnerPassCount",
                                                                 "DataShareAgreementSetOrExemptPassCount","AuthoritativeSourcePassCount","ValidUseCasePassCount","ValidTermsOfUsePassCount",
                                                                 "AccessEntitlementPassCount","LastRefreshedAt")
        businessdomain_trend_df = businessdomain_trend_df.na.fill(value=0)
        
        return businessdomain_trend_df

    def aggregate_business_domain_trends(businessdomain_trend_df):
        #get count before aggregation to add it later
        businessdomain_trend_count = businessdomain_trend_df.count()
        
        businessdomain_trend_df = businessdomain_trend_df.select("BusinessDomainId","DataProductCount","AssetCount","TotalOpenActionsCount","ClassificationPassCount",
                                                                 "GlossaryTermPassCount","DataQualityScorePassCount","DataProductDescriptionPassCount","ValidDataProductOwnerPassCount",
                                                                 "DataShareAgreementSetOrExemptPassCount","AuthoritativeSourcePassCount","ValidUseCasePassCount","ValidTermsOfUsePassCount",
                                                                 "AccessEntitlementPassCount")
        
        businessdomain_trend_df = BusinessDomainTrendTransformations.calculate_aggregated_sum_for_all_columns(businessdomain_trend_df)
        businessdomain_trend_df = HelperFunction.calculate_uuid_column(businessdomain_trend_df,"RowId")
        businessdomain_trend_df = HelperFunction.calculate_last_refreshed_at(businessdomain_trend_df,"LastRefreshedAt")
        businessdomain_trend_df = BusinessDomainTrendTransformations.calculate_businessdomain_count(businessdomain_trend_df,businessdomain_trend_count)
        
        aggregated_businessdomain_trend_df = businessdomain_trend_df.select("RowId","DataProductCount","AssetCount","BusinessDomainCount","TotalOpenActionsCount","ClassificationPassCount",
                                                                 "GlossaryTermPassCount","DataQualityScorePassCount","DataProductDescriptionPassCount","ValidDataProductOwnerPassCount",
                                                                 "DataShareAgreementSetOrExemptPassCount","AuthoritativeSourcePassCount","ValidUseCasePassCount","ValidTermsOfUsePassCount",
                                                                 "AccessEntitlementPassCount","LastRefreshedAt")
        return aggregated_businessdomain_trend_df
