from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from pyspark.sql.functions import *
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.Catalog.Relationship.relationship_constants import RelationshipConstants
from DataEstateHealthLibrary.Catalog.Relationship.relationship_transformations import RelationshipTransformations

class BuildRelationship:
    def build_asset_product_association(relationship_df, dataasset_df, dataproduct_df):
        
        relationship_df = relationship_df.select("SourceId","SourceType" ,"TargetId", "TargetType")
        relationship_df = relationship_df.filter((col("SourceType").isin(RelationshipConstants.IncludeTypes)) | (col("TargetType").isin(RelationshipConstants.IncludeTypes)))
        relationship_df = relationship_df.filter(~((col("SourceType").isin(RelationshipConstants.ExcludeTypes)) | (col("TargetType").isin(RelationshipConstants.ExcludeTypes))))
        relationship_df = RelationshipTransformations.calculate_dataasset_id(relationship_df)
        relationship_df = RelationshipTransformations.calculate_dataproduct_id(relationship_df)
        #relationship_df = ColumnFunctions.rename_col(relationship_df, "TargetId", "DataAssetId")
        #relationship_df = ColumnFunctions.rename_col(relationship_df, "SourceId", "DataProductId")
        
        relationship_df = relationship_df.select("DataAssetId","DataProductId")

        if not dataasset_df.isEmpty():
            dataasset_df = dataasset_df.select("DataAssetId");
            dataasset_df = dataasset_df.join(relationship_df, "DataAssetId", "leftouter")
            
        if not dataproduct_df.isEmpty():
            dataproduct_df = dataproduct_df.select("Id");
            dataproduct_df = ColumnFunctions.rename_col(dataproduct_df, "Id", "DataProductId")
            dataproduct_df = dataproduct_df.join(relationship_df, "DataProductId", "leftouter")
            
        if dataproduct_df.isEmpty() and dataasset_df.isEmpty():
            #remove duplicate rows
            relationship_df = relationship_df.distinct()
            #drop any row which has a null value
            relationship_df = relationship_df.na.drop()
            return relationship_df
        else:
            asset_product_association_df = dataasset_df.unionByName(dataproduct_df,allowMissingColumns=True)
            #remove duplicate rows
            asset_product_association_df = asset_product_association_df.distinct()
            #drop any row which has a null value
            asset_product_association_df = asset_product_association_df.na.drop()
            return asset_product_association_df
    
    def handle_relationship_deletes(relationship_df, deleted_relationship_df):
        if not deleted_relationship_df.isEmpty():    
            relationship_df = relationship_df.join(deleted_relationship_df, ["TargetId","SourceId"], "leftanti")    
        return relationship_df
