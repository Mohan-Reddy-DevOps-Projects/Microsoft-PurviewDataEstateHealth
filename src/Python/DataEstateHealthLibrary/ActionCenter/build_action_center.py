from DataEstateHealthLibrary.ActionCenter.action_center_aggregation import ActionCenterAggregation

class BuildActionCenter:
    
    def build_action_center_schema(dataproduct_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association):
        action_df = ActionCenterAggregation.aggregate_actions(dataproduct_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association)
        action_df = action_df.distinct()
        return action_df
    
    def build_health_action_schema(data_product_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association):
        action_df = ActionCenterAggregation.aggregate_actions(data_product_df,businessdomain_df,dataproduct_contact_association,dataproduct_domain_association,data_asset_df,dataasset_contact_association)
        action_df = ActionCenterAggregation.aggregated_health_action_contact(action_df)
        return action_df
