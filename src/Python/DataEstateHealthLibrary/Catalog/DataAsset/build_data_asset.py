from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_column_functions import DataAssetColumnFunctions
from DataEstateHealthLibrary.Catalog.DataAsset.data_asset_transformations import DataAssetTransformations
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.Shared.dedup_helper_function import DedupHelperFunction
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from pyspark.sql.functions import *

class BuildDataAsset:
    def build_data_asset_schema(dataasset_df):
        #type + typeproperties to derive qualified name
        dataasset_df = CatalogColumnFunctions.add_system_data_schema(dataasset_df)

        dataasset_df = ColumnFunctions.add_new_column_from_col_field(dataasset_df,"SystemData" ,"createdAt", "CreatedAt")
        dataasset_df = ColumnFunctions.add_new_column_from_col_field(dataasset_df,"SystemData" ,"createdBy", "CreatedBy")
        dataasset_df = ColumnFunctions.add_new_column_from_col_field(dataasset_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        dataasset_df = ColumnFunctions.add_new_column_from_col_field(dataasset_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        
        dataasset_df = DataAssetTransformations.calculate_has_description(dataasset_df)
        dataasset_df = DataAssetTransformations.calculate_has_classification(dataasset_df)
        dataasset_df = DataAssetTransformations.calculate_has_schema(dataasset_df)
        dataasset_df = DataAssetColumnFunctions.add_source_schema(dataasset_df)
        dataasset_df = DataAssetTransformations.add_data_asset_id(dataasset_df)
        dataasset_df = DataAssetTransformations.add_source_type(dataasset_df)
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Id", "DataAssetId")
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Domain", "BusinessDomainId")
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Name", "DisplayName")
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Type", "ObjectType")
        dataasset_df = HelperFunction.calculate_last_refreshed_at(dataasset_df,"LastRefreshedAt")

        #add timestamp for deduping
        dataasset_df = CatalogTransformationFunctions.add_timestamp_col(dataasset_df)
        #remove duplicate rows
        dataasset_df = dataasset_df.distinct()
        
        #map by business domain id and reduce by timestamp
        dedup_rdd = dataasset_df.rdd.map(lambda x: (x["DataAssetId"], x))
        dedup_rdd2 = dedup_rdd.reduceByKey(lambda x,y : DedupHelperFunction.dedup_by_timestamp(x,y))
        dedup_rdd3 = dedup_rdd2.map(lambda x: x[1])
        
        #convert it to dataframe with sample ration 1 so as to avoid error with null column values
        final_dataasset_df = dedup_rdd3.toDF(sampleRatio=1.0)
        final_dataasset_df = final_dataasset_df.select("DataAssetId","BusinessDomainId","SourceAssetId","DisplayName","ObjectType","SourceType","HasClassification",
                                                       "HasSchema","HasDescription","CreatedAt","CreatedBy","ModifiedAt","ModifiedBy", "LastRefreshedAt")
        
        return final_dataasset_df

    def build_data_asset_contact_association(dataasset_df):
        dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Id", "DataAssetId")
        dataasset_df = CatalogColumnFunctions.add_contacts_schema(dataasset_df)
        dataasset_owner_df = CatalogTransformationFunctions.format_contact(dataasset_df, "owner", "Owner", "Contacts")

        dataasset_expert_df = CatalogTransformationFunctions.format_contact(dataasset_df, "expert", "Expert", "Contacts")
    
        dataasset_database_admin_df = CatalogTransformationFunctions.format_contact(dataasset_df, "databaseAdmin", "DatabaseAdmin", "Contacts")
    
        union_df = [dataasset_owner_df,dataasset_expert_df,dataasset_database_admin_df]
        merged_df = reduce(DataFrame.unionAll, union_df)
    
        data_asset_contact_association_df = merged_df.select("DataAssetId", "ContactRole", "ContactId","ContactDescription")
        #remove duplicate rows
        data_asset_contact_association_df = data_asset_contact_association_df.distinct()
        #drop any row which has a null value in listed columns
        data_asset_contact_association_df = data_asset_contact_association_df.na.drop(subset=["DataAssetId","ContactId","ContactRole"])
        return data_asset_contact_association_df
    
    def build_asset_domain_association(dataasset_df):
       dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Domain", "BusinessDomainId")
       dataasset_df = ColumnFunctions.rename_col(dataasset_df, "Id", "DataAssetId")
       asset_domain_association_df = dataasset_df.select("BusinessDomainId", "DataAssetId")
       #remove duplicate rows
       asset_domain_association_df = asset_domain_association_df.distinct()
       #drop any row which has a null value
       asset_domain_association_df = asset_domain_association_df.na.drop()
       return asset_domain_association_df
    
    def handle_dataasset_deletes(dataasset_df, deleted_dataasset_df):
        if not deleted_dataasset_df.isEmpty():
            deleted_dataasset_df = deleted_dataasset_df.select("Id");
            dataasset_df = dataasset_df.join(deleted_dataasset_df, ["Id"], "leftanti")
            
        return dataasset_df
    
    def update_boolean_to_int(dataasset_df):
        dataasset_df = HelperFunction.update_col_to_int(dataasset_df,"HasClassification")
        dataasset_df = HelperFunction.update_col_to_int(dataasset_df,"HasSchema")
        dataasset_df = HelperFunction.update_col_to_int(dataasset_df,"HasDescription")
            
        return dataasset_df
