from pyspark.sql.functions import *
from DataEstateHealthLibrary.CdmcScore.cdmc_controls_transformations import CdmcControlsTransformations
from DataEstateHealthLibrary.CdmcScore.score_transformations import ScoreTransformations
from DataEstateHealthLibrary.CdmcScore.score_constants import ScoreConstants
from DataEstateHealthLibrary.CdmcScore.score_schema import ScoreSchema
from DataEstateHealthLibrary.Shared import helper_function
from DataEstateHealthLibrary.Shared.column_functions import ColumnFunctions
from functools import reduce
from pyspark.sql import DataFrame
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

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
        dataproduct_df = CdmcControlsTransformations.add_total_data_products(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.add_use(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.add_data_health(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.add_quality(dataproduct_df)
        dataproduct_df = CdmcControlsTransformations.add_metadata_completeness(dataproduct_df)
        #select only required fields to work with smaller dataset
        dataproduct_df = dataproduct_df.select("BusinessDomainId","TotalDataProducts","C2_Ownership","C3_AuhoritativeSource","C5_Catalog",
                                "C6_Classification","C7_Access","C8_DataConsumptionPurpose","C12_Quality","DataHealth","MetadataCompleteness","Use","Quality")

        
        #snap a dataframe for union of buckets - using this for schema - since we cannot build new df in package as it needs spark config
        aggregated_scores_df = dataproduct_df.limit(1)
        
        #group rows with same business domain id into one
        dataproduct_df = dataproduct_df.groupBy("BusinessDomainId").sum()
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(TotalDataProducts)","TotalDataProducts")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C2_Ownership)","C2_Ownership")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C3_AuhoritativeSource)","C3_AuhoritativeSource")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C5_Catalog)","C5_Catalog")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C6_Classification)","C6_Classification")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C7_Access)","C7_Access")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C8_DataConsumptionPurpose)","C8_DataConsumptionPurpose")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C12_Quality)","C12_Quality")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataHealth)","DataHealth")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(MetadataCompleteness)","MetadataCompleteness")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(Use)","Use")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(Quality)","Quality")
        
        final_aggregated_domain_df = ScoreAggregation.get_aggregation_by_businessdomain_id(dataproduct_df)
        
        #get count for processing later - just an improvisation to avoid scanning dataframe everytime
        totalnumberofDataProducts = final_aggregated_domain_df.count()
        
        #calculate actual score for all domains
        aggregated_scores_df = final_aggregated_domain_df.groupBy().sum()
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(TotalDataProducts)","TotalDataProducts")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C2_Ownership)","C2_Ownership")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C3_AuhoritativeSource)","C3_AuhoritativeSource")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C5_Catalog)","C5_Catalog")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C6_Classification)","C6_Classification")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C7_Access)","C7_Access")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C8_DataConsumptionPurpose)","C8_DataConsumptionPurpose")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C12_Quality)","C12_Quality")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(DataHealth)","DataHealth")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(MetadataCompleteness)","MetadataCompleteness")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Use)","Use")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Quality)","Quality")
        #assigning a default business domain Id to aggregation
        aggregated_scores_df = HelperFunction.calculate_default_business_domain_id(aggregated_scores_df,ScoreConstants.DefaultBusinessDomainId)
        
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

    def get_aggregation_by_businessdomain_id(dataproduct_df):
        dataproduct_df = ScoreTransformations.calculate_data_health_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_metadata_completeness(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_use(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_quality_measure(dataproduct_df)

        return dataproduct_df
    
