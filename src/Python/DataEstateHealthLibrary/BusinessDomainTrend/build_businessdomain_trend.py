from DataEstateHealthLibrary.BusinessDomainTrend.business_domain_trend_aggregation import BusinessDomainTrendAggregation

class BuildBusinessDomainTrend:
    def build_businessdomain_trend_by_id(businessdomain_df, dataproduct_df, productdomain_association_df, assetproduct_association_df,healthaction_df):
        businessdomain_trend_df = BusinessDomainTrendAggregation.aggregate_business_domain_trends_by_id(businessdomain_df, dataproduct_df, productdomain_association_df, assetproduct_association_df, healthaction_df)
        
        return businessdomain_trend_df
        
    def build_businessdomain_trend(businessdomain_trend_df):
        aggregated_businessdomain_trend_df = BusinessDomainTrendAggregation.aggregate_business_domain_trends(businessdomain_trend_df)
        
        return aggregated_businessdomain_trend_df

