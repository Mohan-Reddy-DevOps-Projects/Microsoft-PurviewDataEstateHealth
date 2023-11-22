from DataEstateHealthLibrary.HealthSummary.health_summary_aggregation import HealthSummaryAggregation
from pyspark.sql.functions import *

class BuildHealthSummary:
    
    def build_health_summary_by_domain(businessdomain_df, healthaction_df, productdomain_association_df ,assetproduct_association_df):
        health_summary_by_domain_df = HealthSummaryAggregation.aggregate_health_summary_by_domain(businessdomain_df, healthaction_df, productdomain_association_df ,assetproduct_association_df)
        return health_summary_by_domain_df

    def build_health_summary(health_summary_by_domain_df):
        if not health_summary_by_domain_df.isEmpty():
            healthsummary_df = HealthSummaryAggregation.aggregate_health_summary(health_summary_by_domain_df)
            return healthsummary_df
        return health_summary_by_domain_df
