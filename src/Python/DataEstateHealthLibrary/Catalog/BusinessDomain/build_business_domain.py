from DataEstateHealthLibrary.Catalog.catalog_schema import CatalogSchema
from DataEstateHealthLibrary.Catalog.catalog_column_functions import CatalogColumnFunctions
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_column_functions import BusinessDomainColumnFunctions
from DataEstateHealthLibrary.Catalog.BusinessDomain.business_domain_transformations import BusinessdomainTransformations
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Catalog.catalog_transformation_functions import CatalogTransformationFunctions
from DataEstateHealthLibrary.Catalog.DataProduct.data_product_transformations import DataProductTransformations
from DataEstateHealthLibrary.Catalog.Terms.term_transformations import TermTransformations

class BuildBusinessDomain:
    
    def build_business_domain_schema(businessdomain_df, dataproduct_domain_association_df, term_domain_association_df):

        """ build data product table """
        businessdomain_df = CatalogColumnFunctions.add_system_data_schema(businessdomain_df)
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"createdAt", "CreatedAt")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"createdby", "CreatedBy")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"lastModifiedBy", "ModifiedBy")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"lastModifiedAt", "ModifiedAt")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"expiredAt", "ExpiredAt")
        businessdomain_df = ColumnFunctions.add_new_column_from_col_field(businessdomain_df,"SystemData" ,"expiredBy", "ExpiredBy")

        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "Name", "BusinessDomainDisplayName")
        businessdomain_df = ColumnFunctions.rename_col(businessdomain_df, "Id", "BusinessDomainId")
        #businessdomain_df = BusinessDomainColumnFunctions.add_thumbnail_schema(businessdomain_df)
        businessdomain_df = BusinessdomainTransformations.calculate_is_root_domain(businessdomain_df)
        businessdomain_df = BusinessdomainTransformations.calculate_has_not_null_description(businessdomain_df)
        businessdomain_df = DataProductTransformations.calculate_data_product_count_for_domain(dataproduct_domain_association_df)
        businessdomain_df = TermTransformations.calculate_term_count_for_domain(term_domain_association_df)
        
        businessdomain_df = businessdomain_df.select("BusinessDomainId", "BusinessDomainId", "HasNotNullDescription", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy", "ExpiredAt", "ExpiredBy", "GlossaryTermsCount",
                                                             "DataProductsCount", "IsRootDomain")
        #flattened_data_product_schema.write.mode("overwrite").format("delta").option("mergeSchema", "true").saveAsTable("<account_id>_data_product_schema")
        return businessdomain_df
    
    def build_businessdomain_term_dataproduct_association_schema(dataasset_dataproduct_businessdomain_association_df, term_businessdomain_association_df):
        """ build data product table """

        #need to test
        final_df = dataasset_dataproduct_businessdomain_association_df.join(term_businessdomain_association_df,"BusinessDomainId","fullouter")
        term_businessdomain_dataproduct_dataasset_association_df = final_df.select("BusinessDomainId", "TermId", "DataProductId", "DataAssetId")
        return term_businessdomain_dataproduct_dataasset_association_df
