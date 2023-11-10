from DataEstateHealthLibrary.CdmcControls.cdmc_controls_transformations import CdmcControlsTransformations
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.common_constants import CommonConstants
from functools import reduce
from pyspark.sql import DataFrame

class CdmcControlsAggregation:
    def aggregate_cdmc_controls(dataproduct_df,productdomain_association_df):
        
        #join to get associated domain id for data product
        dataproduct_df = productdomain_association_df.join(dataproduct_df,"DataProductId","leftouter")

        #Add controls to dataproduct df for score calculation
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_2(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_3(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_5(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_6(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_7(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_8(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.calculate_cdmc_control_12(dataproduct_df)
        
        #select only required fields to work with smaller dataset
        dataproduct_df = dataproduct_df.select("BusinessDomainId","C2_Ownership","C3_AuhoritativeSource","C5_Catalog",
                                "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality")
        
        #group rows with same business domain id into one
        final_aggregated_cdmc_control_domain_df = CdmcControlsTransformations.calculate_cdmc_column_sum_by_domain(dataproduct_df)
        final_aggregated_cdmc_control_domain_df = final_aggregated_cdmc_control_domain_df.select("BusinessDomainId","Ownership","AuhoritativeSource","Catalog",
                                "Classification","Access","DataConsumptionPurpose","Quality")
        
        #calculate aggregated cdmc control for all domains
        aggregated_cdmc_controls_df = CdmcControlsTransformations.calculate_aggregated_cdmc_column_sum(final_aggregated_cdmc_control_domain_df)
        #assigning a default business domain Id to aggregation
        aggregated_cdmc_controls_df = HelperFunction.calculate_default_business_domain_id(aggregated_cdmc_controls_df,CommonConstants.DefaultBusinessDomainId)
        
        aggregated_cdmc_controls_df = aggregated_cdmc_controls_df.select("BusinessDomainId","Ownership","AuhoritativeSource","Catalog",
                                "Classification","Access","DataConsumptionPurpose","Quality")
        
        union_df = [aggregated_cdmc_controls_df,final_aggregated_cdmc_control_domain_df]
        cdmc_controls_df = reduce(DataFrame.union, union_df)
        return cdmc_controls_df
