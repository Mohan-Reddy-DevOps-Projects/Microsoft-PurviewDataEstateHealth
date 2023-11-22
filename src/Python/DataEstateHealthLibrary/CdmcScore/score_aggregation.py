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
    def aggregate_score_by_domain(dataproduct_df,productdomain_association_df):
        
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
        dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"TotalDataProducts",1)
        dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"Use",0.0)
        dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"Quality",0.0)
        dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"DataHealth",0.0)
        dataproduct_df = HelperFunction.calculate_default_column_value(dataproduct_df,"MetadataCompleteness",0.0)

        #select only required fields to work with smaller dataset
        dataproduct_df = dataproduct_df.select("BusinessDomainId","TotalDataProducts","C2_Ownership","C3_AuthoritativeSource","C5_Catalog",
                                "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality","DataHealth","MetadataCompleteness","Use","Quality")
        
        #group rows with same business domain id into one
        dataproduct_df = ScoreTransformations.calculate_column_sum_by_domain(dataproduct_df)
        
        final_aggregated_domain_df = ScoreTransformations.get_aggregation_by_businessdomain_id(dataproduct_df)
                
        #final scores transformations
        aggregated_score_by_domain_df = ScoreTransformations.add_score_description(final_aggregated_domain_df,ScoreConstants.Description)
        aggregated_score_by_domain_df = ScoreTransformations.add_score_kind(aggregated_score_by_domain_df,ScoreConstants.Scorekind[0])
        aggregated_score_by_domain_df = ScoreTransformations.add_score_name(aggregated_score_by_domain_df,ScoreConstants.ScoreName)
        aggregated_score_by_domain_df = HelperFunction.calculate_last_refreshed_at(aggregated_score_by_domain_df,"LastRefreshedAt")
        aggregated_score_by_domain_df = ColumnFunctions.add_new_col(aggregated_score_by_domain_df, "DataHealth", "ActualValue")
        
        aggregated_score_by_domain_df = aggregated_score_by_domain_df.select("BusinessDomainId","Description","Name","ScoreKind","ActualValue","TotalDataProducts",
                                                                             "C2_Ownership","C3_AuthoritativeSource","C5_Catalog","C6_Classification","C7_Access",
                                                                             "C8_DataConsumptionPurpose","C12_Quality","DataHealth",
                                                                             "MetadataCompleteness","Use","Quality","LastRefreshedAt")
        return aggregated_score_by_domain_df
    
    def aggregate_score(aggregated_score_by_domain_df):
        
        #calculate actual score for all domains

        aggregated_score_by_domain_df = HelperFunction.calculate_default_column_value(aggregated_score_by_domain_df,"TotalBusinessDomains",1)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_column_sum(aggregated_score_by_domain_df)
        aggregated_scores_df = aggregated_scores_df.select("TotalDataProducts","C2_Ownership","C3_AuthoritativeSource","C5_Catalog",
                        "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality","DataHealth","MetadataCompleteness","Use","Quality","TotalBusinessDomains")
        
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c2(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c3(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c5(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c6(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c7(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c8(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_c12(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_health(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_use(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_metadatacompleteness(aggregated_scores_df)
        aggregated_scores_df = ScoreTransformations.calculate_aggregated_quality(aggregated_scores_df)

        #final scores transformations
        score_df = ScoreTransformations.add_score_description(aggregated_scores_df,ScoreConstants.Description)
        score_df = ScoreTransformations.add_score_kind(score_df,ScoreConstants.Scorekind[0])
        score_df = ScoreTransformations.add_score_name(score_df,ScoreConstants.ScoreName)
        score_df = HelperFunction.calculate_last_refreshed_at(score_df,"LastRefreshedAt")
        score_df = ColumnFunctions.add_new_col(score_df, "DataHealth", "ActualValue")
        score_df = HelperFunction.calculate_uuid_column(score_df, "RowId")
        
        score_df = score_df.select("RowId","Description","Name","ScoreKind","ActualValue","TotalDataProducts",
                                   "C2_Ownership","C3_AuthoritativeSource","C5_Catalog","C6_Classification","C7_Access",
                                    "C8_DataConsumptionPurpose","C12_Quality","DataHealth",
                                    "MetadataCompleteness","Use","Quality","TotalBusinessDomains","LastRefreshedAt")
        return score_df
