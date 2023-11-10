from pyspark.sql.functions import *
from DataEstateHealthLibrary.CdmcScore.score_aggregation import ScoreAggregation

class BuildScores:

    def build_score(dataproduct_df,productdomain_association_df):
        score_df = ScoreAggregation.aggregate_score(dataproduct_df,productdomain_association_df)
        return score_df
    
