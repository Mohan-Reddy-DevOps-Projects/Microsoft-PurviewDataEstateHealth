from DataEstateHealthLibrary.BusinessDomainTrend.business_domain_trend_aggregation import BusinessDomainTrendAggregation

class BuildBusinessDomainTrend:
    def build_businessdomain_trend_schema(businessdomain_df, dataproduct_df, productdomain_association_df, assetdomain_association_df):
        businessdomain_trend_df = BusinessDomainTrendAggregation.aggregate_business_domain_trend(businessdomain_df, dataproduct_df, productdomain_association_df, assetdomain_association_df)
        
        return businessdomain_trend_df
        
        

