import datetime
from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.CdmcControls.cdmc_controls_transformations import CdmcControlsTransformations
from DataEstateHealthLibrary.CdmcScore.score_constants import ScoreConstants

class ScoreTransformations:
        
    def add_score_description(score_df, value):
        score_description_added = score_df.withColumn(
        "Description", lit(value)
        )

        return score_description_added
    
    def add_score_kind(score_df, value):
        score_kind_added = score_df.withColumn(
        "ScoreKind", lit(value)
        )

        return score_kind_added
    
    def add_score_name(score_df, value):
        score_name_added = score_df.withColumn(
        "Name", lit(value)
        )

        return score_name_added
    
    def calculate_c12_score(dataproduct_df):
        c12_score_added = dataproduct_df.withColumn(
        "C12_Quality", lit((col("C12_Quality")/col("TotalDataProducts"))*100)
        )

        return c12_score_added

    def calculate_c2_score(dataproduct_df):
        c2_score_added = dataproduct_df.withColumn(
        "C2_Ownership", lit((col("C2_Ownership")/col("TotalDataProducts"))*100)
        )

        return c2_score_added

    def calculate_c3_score(dataproduct_df):
        c3_score_added = dataproduct_df.withColumn(
        "C3_AuthoritativeSource", lit((col("C3_AuthoritativeSource")/col("TotalDataProducts"))*100)
        )

        return c3_score_added

    def calculate_c5_score(dataproduct_df):
        c5_score_added = dataproduct_df.withColumn(
        "C5_Catalog", lit((col("C5_Catalog")/col("TotalDataProducts"))*100)
        )

        return c5_score_added

    def calculate_c7_score(dataproduct_df):
        c7_score_added = dataproduct_df.withColumn(
        "C7_Access", lit((col("C7_Access")/col("TotalDataProducts"))*100)
        )

        return c7_score_added

    def calculate_c6_score(dataproduct_df):
        c6_score_added = dataproduct_df.withColumn(
        "C6_Classification", lit((col("C6_Classification")/col("TotalDataProducts"))*100)
        )

        return c6_score_added

    def calculate_c8_score(dataproduct_df):
        c8_score_added = dataproduct_df.withColumn(
        "C8_DataConsumptionPurpose", lit((col("C8_DataConsumptionPurpose")/col("TotalDataProducts"))*100)
        )

        return c8_score_added

    def calculate_aggregated_c2(aggregated_scores_df):
        aggregated_c2_added = aggregated_scores_df.withColumn(
        "C2_Ownership", lit(col("C2_Ownership")/col("TotalBusinessDomains"))
        )

        return aggregated_c2_added

    def calculate_aggregated_c3(aggregated_scores_df):
        aggregated_c3_added = aggregated_scores_df.withColumn(
        "C3_AuthoritativeSource", lit(col("C3_AuthoritativeSource")/col("TotalBusinessDomains"))
        )

        return aggregated_c3_added
    
    def calculate_aggregated_c5(aggregated_scores_df):
        aggregated_c5_added = aggregated_scores_df.withColumn(
        "C5_Catalog", lit(col("C5_Catalog")/col("TotalBusinessDomains"))
        )

        return aggregated_c5_added

    def calculate_aggregated_c6(aggregated_scores_df):
        aggregated_c6_added = aggregated_scores_df.withColumn(
        "C6_Classification", lit(col("C6_Classification")/col("TotalBusinessDomains"))
        )

        return aggregated_c6_added

    def calculate_aggregated_c7(aggregated_scores_df):
        aggregated_c7_added = aggregated_scores_df.withColumn(
        "C7_Access", lit(col("C7_Access")/col("TotalBusinessDomains"))
        )

        return aggregated_c7_added

    def calculate_aggregated_c8(aggregated_scores_df):
        aggregated_c8_added = aggregated_scores_df.withColumn(
        "C8_DataConsumptionPurpose", lit(col("C8_DataConsumptionPurpose")/col("TotalBusinessDomains"))
        )

        return aggregated_c8_added

    def calculate_aggregated_c12(aggregated_scores_df):
        aggregated_c12_added = aggregated_scores_df.withColumn(
        "C12_Quality", lit(col("C12_Quality")/col("TotalBusinessDomains"))
        )

        return aggregated_c12_added
    
    def calculate_aggregated_health(aggregated_scores_df):
        aggregated_health_added = aggregated_scores_df.withColumn(
        "DataHealth", lit(col("DataHealth")/col("TotalBusinessDomains"))
        )

        return aggregated_health_added

    def calculate_aggregated_use(aggregated_scores_df):
        aggregated_use_added = aggregated_scores_df.withColumn(
        "Use", lit(col("Use")/col("TotalBusinessDomains"))
        )

        return aggregated_use_added

    def calculate_aggregated_metadatacompleteness(aggregated_scores_df):
        #sum_MetadataCompleteness = final_aggregated_domain_df.agg({"MetadataCompleteness":"sum"}).collect()[0]
        #result_MetadataCompleteness= int(sum_MetadataCompleteness["sum(MetadataCompleteness)"])
        #average = result_MetadataCompleteness/totalnumberofDataProducts
        aggregated_metadatacompleteness_added = aggregated_scores_df.withColumn(
        "MetadataCompleteness", lit(col("MetadataCompleteness")/col("TotalBusinessDomains"))
        )

        return aggregated_metadatacompleteness_added

    def calculate_aggregated_quality(aggregated_scores_df):
        aggregated_quality_added = aggregated_scores_df.withColumn(
        "Quality", lit(col("Quality")/col("TotalBusinessDomains"))
        )

        return aggregated_quality_added
    
    #Quality
    def calculate_quality_measure(dataproduct_df):
        quality_score_added = dataproduct_df.withColumn(
        "Quality", lit(((col("C12_Quality")+col("C3_AuthoritativeSource"))/2)/col("TotalDataProducts"))
            )

        return quality_score_added

    #Use
    def calculate_use(dataproduct_df):
        use_score_added = dataproduct_df.withColumn(
        "Use", lit(((col("C7_Access")+col("C8_DataConsumptionPurpose"))/2)/col("TotalDataProducts"))
        )

        return use_score_added

    #Metadata completeness
    def calculate_metadata_completeness(dataproduct_df):
        metadata_completeness_score_added = dataproduct_df.withColumn(
        "MetadataCompleteness", lit(((col("C2_Ownership")+col("C5_Catalog")+col("C6_Classification"))/3)/col("TotalDataProducts"))
            )

        return metadata_completeness_score_added

    #Data Health score    
    def calculate_data_health_score(dataproduct_df):
        data_health_score_added = dataproduct_df.withColumn(
            "DataHealth", lit((col("C12_Quality")+col("C2_Ownership")+col("C3_AuthoritativeSource")+col("C5_Catalog")+col("C7_Access")+
                                col("C8_DataConsumptionPurpose")+col("C6_Classification"))/(col("TotalDataProducts")*7))
                        )

        return data_health_score_added
    
    def get_aggregation_by_businessdomain_id(dataproduct_df):
        dataproduct_df = ScoreTransformations.calculate_c2_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c3_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c5_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c6_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c7_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c8_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_c12_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_metadata_completeness(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_use(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_quality_measure(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_data_health_score(dataproduct_df)

        return dataproduct_df
    
    def calculate_column_sum_by_domain(dataproduct_df):
        dataproduct_df = dataproduct_df.groupBy("BusinessDomainId").sum()
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(TotalDataProducts)","TotalDataProducts")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C2_Ownership)","C2_Ownership")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C3_AuthoritativeSource)","C3_AuthoritativeSource")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C5_Catalog)","C5_Catalog")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C6_Classification)","C6_Classification")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C7_Access)","C7_Access")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C8_DataConsumptionPurpose)","C8_DataConsumptionPurpose")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C12_Quality)","C12_Quality")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(DataHealth)","DataHealth")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(MetadataCompleteness)","MetadataCompleteness")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(Use)","Use")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(Quality)","Quality")
        return dataproduct_df

    def calculate_aggregated_column_sum(final_aggregated_domain_df):
        aggregated_scores_df = final_aggregated_domain_df.groupBy().sum()
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(TotalDataProducts)","TotalDataProducts")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C2_Ownership)","C2_Ownership")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C3_AuthoritativeSource)","C3_AuthoritativeSource")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C5_Catalog)","C5_Catalog")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C6_Classification)","C6_Classification")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C7_Access)","C7_Access")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C8_DataConsumptionPurpose)","C8_DataConsumptionPurpose")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(C12_Quality)","C12_Quality")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(DataHealth)","DataHealth")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(MetadataCompleteness)","MetadataCompleteness")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Use)","Use")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Quality)","Quality")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(TotalBusinessDomains)","TotalBusinessDomains")
        return aggregated_scores_df
