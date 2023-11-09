from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

class BuildRelationship:
    def build_product_asset_domain_association(relationship_df):
        product_asset_association_df = ColumnFunctions.rename_col(relationship_df, "TargetId", "DataAssetId")
        product_asset_association_df = ColumnFunctions.rename_col(product_asset_association_df, "SourceId", "DataProductId")
        
        product_asset_association_df = product_asset_association_df.select("DataAssetId","DataProductId")
        #remove duplicate rows
        product_asset_association_df = product_asset_association_df.distinct()
        #drop any row which has a null value
        product_asset_association_df = product_asset_association_df.na.drop()
        return product_asset_association_df
