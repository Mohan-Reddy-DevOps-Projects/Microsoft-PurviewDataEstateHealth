from DataEstateHealthLibrary.HealthSummary.health_summary_aggregation import HealthSummaryAggregation
from pyspark.sql.functions import *
class BuildHealthSummary:
    
    def build_health_summary(businessdomain_df, healthaction_df, assetdomain_association_df):
        health_summary_df = HealthSummaryAggregation.aggregate_health_summary(businessdomain_df, healthaction_df, assetdomain_association_df)
        health_summary_df = health_summary_df.distinct()
        return health_summary_df
