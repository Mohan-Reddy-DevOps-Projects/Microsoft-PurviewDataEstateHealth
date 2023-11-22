from DataEstateHealthLibrary.CdmcControls.cdmc_controls_aggregation import CdmcControlsAggregation

class BuildCdmcControls:
    def build_cdmc_controls_by_domain(dataproduct_df,productdomain_association_df):
        cdmc_controls_df_by_domain = CdmcControlsAggregation.aggregate_cdmc_controls_by_domain(dataproduct_df,productdomain_association_df)
        return cdmc_controls_df_by_domain

    def build_cdmc_controls(cdmc_controls_df_by_domain):
        cdmc_controls_df = CdmcControlsAggregation.aggregate_cdmc_controls(cdmc_controls_df_by_domain)
        return cdmc_controls_df
