from pyspark.sql.functions import *
from DataEstateHealthLibrary.CdmcScore.score_aggregation import ScoreAggregation

class BuildScores:

    def build_score_by_domain(dataproduct_df,productdomain_association_df):
        score_by_domain_df = ScoreAggregation.aggregate_score_by_domain(dataproduct_df,productdomain_association_df)
        return score_by_domain_df
    
    def build_score(score_by_domain_df):
        score_df = ScoreAggregation.aggregate_score(score_by_domain_df)
        return score_df
    
