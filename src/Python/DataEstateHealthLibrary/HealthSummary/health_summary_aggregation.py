from functools import reduce
from pyspark.sql import DataFrame
from pyspark.sql.functions import *
from DataEstateHealthLibrary.HealthSummary.health_summary_constants import HealthSummaryConstants
from DataEstateHealthLibrary.HealthSummary.health_summary_transformation import HealthSummaryTransformation
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.common_constants import CommonConstants
from DataEstateHealthLibrary.Shared.shared_transformations import SharedTransformations

class HealthSummaryAggregation:
    def aggregate_health_summary_by_domain(businessdomain_df, healthaction_df, productdomain_association_df ,assetproduct_association_df):
        
        if assetproduct_association_df.isEmpty() or productdomain_association_df.isEmpty():
            businessdomain_df = HelperFunction.calculate_default_column_value(businessdomain_df,"TotalCuratedDataAssetsCount", 0)
        else:
            businessdomain_df = SharedTransformations.calculate_asset_count(businessdomain_df, productdomain_association_df ,
                                                                                          assetproduct_association_df,"TotalCuratedDataAssetsCount") 

        if not healthaction_df.isEmpty():
            healthaction_df = healthaction_df.select("ActionId","BusinessDomainId")
            healthaction_df = CatalogColumnFunctions.calculate_count_for_domain(healthaction_df,"TotalOpenActionsCount")
            businessdomain_df = businessdomain_df.join(healthaction_df,"BusinessDomainId","leftouter")
        else:
            businessdomain_df = HelperFunction.calculate_default_column_value(businessdomain_df,"TotalOpenActionsCount",0)
        
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df,"DataProductsCount","TotalDataProductsCount")
        businessdomain_df = HealthSummaryTransformation.calculate_total_data_products_count(businessdomain_df) 
        businessdomain_df = HealthSummaryTransformation.default_null_curated_data_asset_count_to_zero(businessdomain_df)
        businessdomain_df = HelperFunction.calculate_default_column_value(businessdomain_df,"TotalBusinessDomains",1)

        businessdomain_df = HealthSummaryTransformation.calculate_business_domains_filter_list_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_business_domains_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_data_assets_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_health_actions_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_data_products_trend_link(businessdomain_df)
        businessdomain_df = businessdomain_df.select("BusinessDomainId","BusinessDomainDisplayName","TotalBusinessDomains","TotalDataProductsCount","TotalCuratedDataAssetsCount",
                                                     "TotalOpenActionsCount" ,"BusinessDomainsFilterListLink","BusinessDomainsTrendLink","DataProductsTrendLink",
                                               "DataAssetsTrendLink","HealthActionsTrendLink")
        
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_total_reports_count(businessdomain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_active_reports_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_draft_reports_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_total_curatable_data_asset_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_total_non_curatable_data_asset_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_total_completed_actions_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HealthSummaryTransformation.calculate_total_dismissed_actions_count(health_summary_by_domain_df)
        health_summary_by_domain_df = HelperFunction.calculate_last_refreshed_at(health_summary_by_domain_df,"LastRefreshedAt")
    
        return health_summary_by_domain_df

    def aggregate_health_summary(health_summary_by_domain_df):
        
        #build a default aggregated health summary
        health_summary_by_domain_df = health_summary_by_domain_df.select("BusinessDomainId","TotalBusinessDomains","TotalDataProductsCount","TotalCuratedDataAssetsCount","TotalOpenActionsCount")

        #calculate total sum
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_aggregated_column_sum(health_summary_by_domain_df)

        #get aggregated row
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_business_domains_filter_list_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_business_domains_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_data_products_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_data_assets_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_health_actions_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HelperFunction.calculate_uuid_column(aggregated_healthsummary_df, "RowId")
        aggregated_healthsummary_df = aggregated_healthsummary_df.select("RowId","TotalBusinessDomains","TotalDataProductsCount","TotalCuratedDataAssetsCount",
                                                     "TotalOpenActionsCount" ,"BusinessDomainsFilterListLink","BusinessDomainsTrendLink","DataProductsTrendLink",
                                               "DataAssetsTrendLink","HealthActionsTrendLink")
        
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_reports_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_active_reports_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_draft_reports_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_curatable_data_asset_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_non_curatable_data_asset_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_completed_actions_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_dismissed_actions_count(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HelperFunction.calculate_last_refreshed_at(aggregated_healthsummary_df,"LastRefreshedAt")
        
        return aggregated_healthsummary_df
