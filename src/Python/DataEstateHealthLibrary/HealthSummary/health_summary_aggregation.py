from functools import reduce
from pyspark.sql import DataFrame
from pyspark.sql.types import *
from DataEstateHealthLibrary.HealthSummary.health_summary_constants import HealthSummaryConstants
from DataEstateHealthLibrary.HealthSummary.health_summary_transformation import HealthSummaryTransformation
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.common_constants import CommonConstants

class HealthSummaryAggregation:
    def aggregate_health_summary(businessdomain_df, healthaction_df, assetdomain_association_df):
        
        #build a default aggregated health summary
        aggregated_healthsummary_df = businessdomain_df.limit(1)
        
        businessdomain_asset_count = CatalogColumnFunctions.calculate_count_for_domain(assetdomain_association_df, "TotalCuratedDataAssetsCount")
        businessdomain_asset_count = businessdomain_asset_count.select("BusinessDomainId","TotalCuratedDataAssetsCount")
        
        businessdomain_df = businessdomain_df.join(businessdomain_asset_count,"BusinessDomainId","leftouter")
        
        healthaction_df = healthaction_df.select("ActionId","BusinessDomainId")
        healthaction_df = CatalogColumnFunctions.calculate_count_for_domain(healthaction_df,"TotalOpenActionsCount")
        #healthaction_df = healthaction_df.groupBy("BusinessDomainId").count()
        #healthaction_df = ColumnFunctions.rename_col(healthaction_df, "count", "TotalOpenActionsCount")
        
        businessdomain_df = businessdomain_df.join(healthaction_df,"BusinessDomainId","leftouter")
        
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df,"DataProductsCount","TotalDataProductsCount")
        businessdomain_df = HealthSummaryTransformation.calculate_total_data_products_count(businessdomain_df) 
        businessdomain_df = HealthSummaryTransformation.default_null_curated_data_asset_count_to_zero(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_total_business_domains(businessdomain_df)

        businessdomain_df = HealthSummaryTransformation.calculate_business_domains_filter_list_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_business_domains_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_data_assets_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_health_actions_trend_link(businessdomain_df)
        businessdomain_df = HealthSummaryTransformation.calculate_data_products_trend_link(businessdomain_df)
        businessdomain_df = businessdomain_df.select("BusinessDomainId","BusinessDomainDisplayName","TotalBusinessDomains", "BusinessDomainsFilterListLink","BusinessDomainsTrendLink","DataProductsTrendLink",
                                               "DataAssetsTrendLink","HealthActionsTrendLink","TotalDataProductsCount","TotalOpenActionsCount",
                                               "TotalCuratedDataAssetsCount")

        #get aggregated row
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_business_domains_filter_list_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_business_domains_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_data_assets_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_health_actions_trend_link(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_total_business_domains(aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_default_data_products_trend_link(aggregated_healthsummary_df)
        #assigning a default business domain Id and display name to aggregation
        aggregated_healthsummary_df = HelperFunction.calculate_default_business_domain_id(aggregated_healthsummary_df, CommonConstants.DefaultBusinessDomainId)
        aggregated_healthsummary_df = HelperFunction.calculate_default_business_domain_dispaly_name(aggregated_healthsummary_df, HealthSummaryConstants.DefaultBusinessDomainDisplayName)
        
        #calculate total sum
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_total_business_domains_count(businessdomain_df,aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_aggregated_data_products_count(businessdomain_df,aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_aggregated_curated_data_asset_count(businessdomain_df,aggregated_healthsummary_df)
        aggregated_healthsummary_df = HealthSummaryTransformation.calculate_aggregated_open_actions_count(businessdomain_df,aggregated_healthsummary_df)
        aggregated_healthsummary_df = aggregated_healthsummary_df.select("BusinessDomainId","BusinessDomainDisplayName","TotalBusinessDomains", "BusinessDomainsFilterListLink","BusinessDomainsTrendLink","DataProductsTrendLink",
                                               "DataAssetsTrendLink","HealthActionsTrendLink","TotalDataProductsCount","TotalOpenActionsCount",
                                               "TotalCuratedDataAssetsCount")
        
        
        union_df = [aggregated_healthsummary_df,businessdomain_df]
        health_summary_df = reduce(DataFrame.union, union_df)
        health_summary_df = HealthSummaryTransformation.calculate_total_reports_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_active_reports_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_draft_reports_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_total_curatable_data_asset_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_total_non_curatable_data_asset_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_total_completed_actions_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_total_dismissed_actions_count(health_summary_df)
        health_summary_df = HealthSummaryTransformation.calculate_last_refresh_date(health_summary_df)
        health_summary_df = health_summary_df.withColumn("TotalCuratedDataAssetsCount",health_summary_df.TotalCuratedDataAssetsCount.cast(IntegerType()))
    
        return health_summary_df
