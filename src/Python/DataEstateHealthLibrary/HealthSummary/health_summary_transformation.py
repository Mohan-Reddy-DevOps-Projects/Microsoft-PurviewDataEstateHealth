from pyspark.sql.functions import *
from DataEstateHealthLibrary.HealthSummary.health_summary_constants import HealthSummaryConstants
import datetime

class HealthSummaryTransformation:

    def calculate_default_total_business_domains(aggregated_healthsummary_df):
        default_total_business_domains_added = aggregated_healthsummary_df.withColumn(
            "TotalBusinessDomains", lit(0)
        )

        return default_total_business_domains_added
    
    def calculate_default_business_domains_filter_list_link(aggregated_healthsummary_df):
        business_domains_filter_list_link_added = aggregated_healthsummary_df.withColumn(
            "BusinessDomainsFilterListLink", concat(lit(HealthSummaryConstants.BUSINESS_DOMAINS_FILTERS_LIST_LINK))
        )

        return business_domains_filter_list_link_added

    def calculate_default_business_domains_trend_link(aggregated_healthsummary_df):
        business_domains_trend_link_added = aggregated_healthsummary_df.withColumn(
            "BusinessDomainsTrendLink", lit(HealthSummaryConstants.BUSINESS_DOMAINS_TREND_LINK)
        )

        return business_domains_trend_link_added
    
    def calculate_default_data_products_trend_link(aggregated_healthsummary_df):
        data_products_trend_link_added = aggregated_healthsummary_df.withColumn(
            "DataProductsTrendLink", lit(HealthSummaryConstants.DATA_PRODUCTS_TREND_LINK)
        )

        return data_products_trend_link_added

    def calculate_default_data_assets_trend_link(aggregated_healthsummary_df):
        data_assets_trend_link_added = aggregated_healthsummary_df.withColumn(
            "DataAssetsTrendLink", lit(HealthSummaryConstants.DATA_ASSETS_TREND_LINK)
        )

        return data_assets_trend_link_added
    
    def calculate_default_health_actions_trend_link(aggregated_healthsummary_df):
        health_actions_trend_link_added = aggregated_healthsummary_df.withColumn(
            "HealthActionsTrendLink", lit(HealthSummaryConstants.HEALTH_ACTIONS_TREND_LINK)
        )

        return health_actions_trend_link_added
       
    def calculate_business_domains_filter_list_link(businessdomain_df):
        business_domains_filter_list_link_added = businessdomain_df.withColumn(
            "BusinessDomainsFilterListLink", concat(lit(HealthSummaryConstants.BUSINESS_DOMAINS_FILTERS_LIST_LINK), col("BusinessDomainId"))
        )

        return business_domains_filter_list_link_added

    def calculate_business_domains_trend_link(businessdomain_df):
        business_domains_trend_link_added = businessdomain_df.withColumn(
            "BusinessDomainsTrendLink", concat(lit(HealthSummaryConstants.BUSINESS_DOMAINS_TREND_LINK), col("BusinessDomainId"))
        )

        return business_domains_trend_link_added
   
    def calculate_data_products_trend_link(businessdomain_df):
        data_products_trend_link_added = businessdomain_df.withColumn(
            "DataProductsTrendLink", concat(lit(HealthSummaryConstants.DATA_PRODUCTS_TREND_LINK), col("BusinessDomainId"))
        )

        return data_products_trend_link_added
    
    def calculate_data_assets_trend_link(businessdomain_df):
        data_assets_trend_link_added = businessdomain_df.withColumn(
            "DataAssetsTrendLink", concat(lit(HealthSummaryConstants.DATA_ASSETS_TREND_LINK), col("BusinessDomainId"))
        )

        return data_assets_trend_link_added
    
    def calculate_health_actions_trend_link(businessdomain_df):
        health_actions_trend_link_added = businessdomain_df.withColumn(
            "HealthActionsTrendLink", concat(lit(HealthSummaryConstants.HEALTH_ACTIONS_TREND_LINK), col("BusinessDomainId"))
        )

        return health_actions_trend_link_added
    
    def calculate_total_business_domains(businessdomain_df):
        total_business_domains_added = businessdomain_df.withColumn(
            "TotalBusinessDomains", lit(1)
        )

        return total_business_domains_added

    def calculate_total_open_actions(businessdomain_df):
        total_open_actions_added = businessdomain_df.withColumn(
            "TotalOpenActionsCount", when(col("ActionId").isNotNull(),lit(1))
            .otherwise(lit(0))
        )

        return total_open_actions_added

    def calculate_total_business_domains_count(businessdomain_df, aggregated_healthsummary_df):
        aggregated_total_business_domains_added = aggregated_healthsummary_df.withColumn(
            "TotalBusinessDomains", lit(businessdomain_df.count())
        )

        return aggregated_total_business_domains_added

    def calculate_total_data_products_count(businessdomain_df):
        total_data_products_added = businessdomain_df.withColumn(
            "TotalDataProductsCount", when(businessdomain_df.TotalDataProductsCount.isNull(), lit(0))
            .otherwise(businessdomain_df.TotalDataProductsCount)
            )

        return total_data_products_added
    
    def default_null_curated_data_asset_count_to_zero(businessdomain_df):
        total_curated_data_asset_added = businessdomain_df.withColumn(
            "TotalCuratedDataAssetsCount", when(businessdomain_df.TotalCuratedDataAssetsCount.isNull(), lit(0))
            .otherwise(businessdomain_df.TotalCuratedDataAssetsCount)
            )

        return total_curated_data_asset_added
    
    def calculate_aggregated_data_products_count(businessdomain_df, aggregated_healthsummary_df):
        sum_TotalDataProductsCount = businessdomain_df.agg({"TotalDataProductsCount":"sum"}).collect()[0]
        result = int(sum_TotalDataProductsCount["sum(TotalDataProductsCount)"])
        aggregated_total_data_products_added = aggregated_healthsummary_df.withColumn(
            "TotalDataProductsCount", lit(result)
        )
        return aggregated_total_data_products_added
    
    def calculate_aggregated_curated_data_asset_count(businessdomain_df, aggregated_healthsummary_df):
        sum_TotalCuratedDataAssetsCount = businessdomain_df.agg({"TotalCuratedDataAssetsCount":"sum"}).collect()[0]
        result = int(sum_TotalCuratedDataAssetsCount["sum(TotalCuratedDataAssetsCount)"])
        aggregated_total_curated_data_asset_added = aggregated_healthsummary_df.withColumn(
            "TotalCuratedDataAssetsCount", lit(result)
        )

        return aggregated_total_curated_data_asset_added
    
    def calculate_aggregated_open_actions_count(businessdomain_df, aggregated_healthsummary_df):
        sum_TotalOpenActionsCount = businessdomain_df.agg({"TotalOpenActionsCount":"sum"}).collect()[0]
        result = int(sum_TotalOpenActionsCount["sum(TotalOpenActionsCount)"])
        aggregated_total_open_actions_added = aggregated_healthsummary_df.withColumn(
            "TotalOpenActionsCount", lit(result)
        )

        return aggregated_total_open_actions_added

    def calculate_total_reports_count(health_summary_df):
        total_reports_count_added = health_summary_df.withColumn(
            "TotalReportsCount", lit(8)
        )

        return total_reports_count_added
    
    def calculate_active_reports_count(health_summary_df):
        active_reports_count_added = health_summary_df.withColumn(
            "ActiveReportsCount", lit(8)
        )

        return active_reports_count_added
    
    def calculate_draft_reports_count(health_summary_df):
        draft_reports_count_added = health_summary_df.withColumn(
            "DraftReportsCount", lit(0)
        )

        return draft_reports_count_added

    def calculate_total_curatable_data_asset_count(health_summary_df):
        total_curatable_data_asset_added = health_summary_df.withColumn(
            "TotalCuratableDataAssetsCount", lit(-1)
        )

        return total_curatable_data_asset_added
    
    def calculate_total_non_curatable_data_asset_count(health_summary_df):
        total_non_curatable_data_asset_added = health_summary_df.withColumn(
            "TotalNonCuratableDataAssetsCount", lit(-1)
        )

        return total_non_curatable_data_asset_added
    
    def calculate_total_completed_actions_count(health_summary_df):
        aggregated_total_completed_actions_added = health_summary_df.withColumn(
            "TotalCompletedActionsCount", lit(-1)
        )

        return aggregated_total_completed_actions_added
    
    def calculate_total_dismissed_actions_count(health_summary_df):
        total_dismissed_actions_added = health_summary_df.withColumn(
            "TotalDismissedActionsCount", lit(-1)
        )

        return total_dismissed_actions_added
    
    def calculate_last_refresh_date(action_center_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        
        last_refresh_date_added = action_center_df.withColumn(
            "LastRefreshDate", lit(date_int)
        )

        return last_refresh_date_added
