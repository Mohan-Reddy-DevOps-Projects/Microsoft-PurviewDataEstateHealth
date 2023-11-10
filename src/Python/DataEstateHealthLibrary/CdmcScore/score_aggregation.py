from DataEstateHealthLibrary.CdmcControls.cdmc_controls_transformations import CdmcControlsTransformations
from DataEstateHealthLibrary.CdmcScore.score_transformations import ScoreTransformations
from DataEstateHealthLibrary.CdmcScore.score_constants import ScoreConstants
from DataEstateHealthLibrary.CdmcScore.score_schema import ScoreSchema
from DataEstateHealthLibrary.Shared import helper_function
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction
from DataEstateHealthLibrary.Shared.common_constants import CommonConstants
from functools import reduce
from pyspark.sql import DataFrame

class ScoreAggregation:
    def aggregate_score(dataproduct_df,productdomain_association_df):
        
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
        dataproduct_df = ScoreTransformations.add_total_data_products(dataproduct_df)
        dataproduct_df = ScoreTransformations.add_use(dataproduct_df)
        dataproduct_df = ScoreTransformations.add_data_health(dataproduct_df)
        dataproduct_df = ScoreTransformations.add_quality(dataproduct_df)
        dataproduct_df = ScoreTransformations.add_metadata_completeness(dataproduct_df)
        #select only required fields to work with smaller dataset
        dataproduct_df = dataproduct_df.select("BusinessDomainId","TotalDataProducts","C2_Ownership","C3_AuhoritativeSource","C5_Catalog",
                                "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality","DataHealth","MetadataCompleteness","Use","Quality")

        
        #snap a dataframe for union of buckets - using this for schema - since we cannot build new df in package as it needs spark config
        aggregated_scores_df = dataproduct_df.limit(1)
        
        #group rows with same business domain id into one
        dataproduct_df = ScoreTransformations.calculate_column_sum_by_domain(dataproduct_df)
        
        final_aggregated_domain_df = ScoreTransformations.get_aggregation_by_businessdomain_id(dataproduct_df)
                
        #calculate actual score for all domains
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_column_sum(final_aggregated_domain_df)
        #assigning a default business domain Id to aggregation
        aggregated_scores_df = HelperFunction.calculate_default_business_domain_id(aggregated_scores_df,CommonConstants.DefaultBusinessDomainId)
        
        aggregated_scores_df = aggregated_scores_df.select("BusinessDomainId","TotalDataProducts","C2_Ownership","C3_AuhoritativeSource","C5_Catalog",
                        "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality","DataHealth","MetadataCompleteness","Use","Quality")
        
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_health(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_use(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_metadatacompleteness(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_quality(aggregated_scores_df)

        union_df = [aggregated_scores_df,final_aggregated_domain_df]
        score_df = reduce(DataFrame.union, union_df)

        #final scores transformations
        score_df = ScoreTransformations.add_score_description(score_df,ScoreConstants.Description)
        score_df = ScoreTransformations.add_score_kind(score_df,ScoreConstants.Scorekind[0])
        score_df = ScoreTransformations.add_score_name(score_df,ScoreConstants.ScoreName)
        score_df = ScoreTransformations.calculate_last_refresh_date(score_df)
        score_df = ColumnFunctions.rename_col(score_df, "DataHealth", "ActualValue")
        
        score_df = score_df.select("BusinessDomainId","Description","Name","ScoreKind","ActualValue","LastRefreshDate")
        return score_df
    
