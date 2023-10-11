from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_column_functions import DataAssetColumnFunctions
from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_transformations import DataAssetTransformations
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions

class BuildDataAsset:
    
    def build_data_asset_schema(dataasset_df):
        #dataproduct_df = spark.createDataFrame(data=DataProductSampleData.data_product_sample_dat_1,schema=CatalogSchema.data_product_schema)

        """ build data product table """
        dataasset_df = CatalogColumnFunctions.add_contacts_schema(dataasset_df)
        dataasset_df = CatalogColumnFunctions.add_system_data_schema(dataasset_df)
        
        dataasset_df = DataAssetColumnFunctions.add_source_schema(dataasset_df)
        dataasset_df = DataAssetColumnFunctions.add_schema_entity_schema_added(dataasset_df)
        #dataasset_df = DataAssetColumnFunctions.add_type_properties_scehma(dataasset_df)
        
        dataasset_df = CatalogColumnFunctions.add_created_at(dataasset_df)
        dataasset_df = CatalogColumnFunctions.add_created_by(dataasset_df)
        dataasset_df = CatalogColumnFunctions.add_modified_by(dataasset_df)
        dataasset_df = CatalogColumnFunctions.add_modified_at(dataasset_df)
    
        
        #dataasset_df_schema = dataproduct_df.select(DataProductConfig.select_string)
        #dataasset_df_schema.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("<account_id>_data_product_schema")
        return dataasset_df

    def build_dataasset_dataproduct_businessdomain_association(dataasset_df):
       dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Id", "DataProductId")
       dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Domain", "BusinessDomainId")
       dataasset_df = DataAssetTransformations.add_data_asset_id(dataasset_df)
       dataasset_dataproduct_businessdomain_association_df = dataasset_df.select("BusinessDomainId","DataProductId", "DataAssetId")
       #final_data_asset_product_association.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("<account_id>_data_asset_product_association_schema")
       return dataasset_dataproduct_businessdomain_association_df
