from DataEstateHealthLibrary.CdmcControls.cdmc_controls_aggregation import CdmcControlsAggregation

class BuildCdmcControls:
    def build_cdmc_controls(dataproduct_df,productdomain_association_df):
        score_df = CdmcControlsAggregation.aggregate_cdmc_controls(dataproduct_df,productdomain_association_df)
        return score_df
