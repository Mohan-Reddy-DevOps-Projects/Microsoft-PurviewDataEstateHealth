from pyspark.sql.functions import *
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class SharedTransformations:
        
        def calculate_asset_count(businessdomain_df, productdomain_association ,assetproduct_association_df, colName):
            businessdomain_df = businessdomain_df.join(productdomain_association, "BusinessDomainId", "leftouter")
            businessdomain_df = businessdomain_df.join(assetproduct_association_df, "DataProductId", "leftouter")
            businessdomain_asset_count = businessdomain_df.select("BusinessDomainId","DataAssetId").filter(col("DataAssetId").isNotNull())
            businessdomain_asset_count = CatalogColumnFunctions.calculate_count_for_domain(businessdomain_asset_count, colName)
            businessdomain_df = businessdomain_df.drop(col("DataProductId")).drop("DataAssetId").distinct()
            businessdomain_df = businessdomain_df.join(businessdomain_asset_count,"BusinessDomainId","leftouter")
            businessdomain_df = HelperFunction.update_null_values(businessdomain_df, colName, 0)
            return businessdomain_df
        
        def calculate_assetproductdomain_association(dataproduct_domain_association ,assetproduct_association_df):
            dataproduct_domain_association = dataproduct_domain_association.drop(col("IsPrimaryDataProduct"))
            assetproductdomain_association = dataproduct_domain_association.join(assetproduct_association_df, "DataProductId", "fullouter")
            assetproductdomain_association = HelperFunction.update_null_values(assetproductdomain_association, "BusinessDomainId", "")
            assetproductdomain_association = HelperFunction.update_null_values(assetproductdomain_association, "DataProductId", "")
            assetproductdomain_association = HelperFunction.update_null_values(assetproductdomain_association, "DataAssetId", "")
            return assetproductdomain_association
