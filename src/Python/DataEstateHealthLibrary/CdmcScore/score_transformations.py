import datetime
from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.CdmcControls.cdmc_controls_transformations import CdmcControlsTransformations
from DataEstateHealthLibrary.CdmcScore.score_constants import ScoreConstants

class ScoreTransformations:
    
    def add_total_data_products(dataproduct_df):
        total_data_products_added = dataproduct_df.withColumn(
        "TotalDataProducts", lit(1)
        )

        return total_data_products_added
    
    def add_actual_value(dataproduct_df):
        data_health_added = dataproduct_df.withColumn(
        "ActualValue", lit(0)
        )

        return data_health_added

    def add_data_health(dataproduct_df):
        data_health_added = dataproduct_df.withColumn(
        "DataHealth", lit(0)
        )

        return data_health_added

    def add_quality(dataproduct_df):
        quality_added = dataproduct_df.withColumn(
        "Quality", lit(0)
        )

        return quality_added

    def add_use(dataproduct_df):
        use_added = dataproduct_df.withColumn(
        "Use", lit(0)
        )

        return use_added

    def add_metadata_completeness(dataproduct_df):
        metadata_completeness_added = dataproduct_df.withColumn(
        "MetadataCompleteness", lit(0)
        )

        return metadata_completeness_added

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
    
    def calculate_last_refresh_date(score_df):
        now = datetime.datetime.now()
        date_string = now.strftime("%Y%m%d")
        date_int = int(date_string)
        
        last_refresh_date_added = score_df.withColumn(
            "LastRefreshDate", lit(date_int)
        )

        return last_refresh_date_added
    
    def calculate_total_c12(dataproduct_df,aggregated_scores_df):
        sum_C12_Quality = dataproduct_df.agg({"C12_Quality":"sum"}).collect()[0]
        result_C12_Quality = int(sum_C12_Quality["sum(C12_Quality)"])
    
        total_c12_added = aggregated_scores_df.withColumn(
        "C12_Quality", lit(result_C12_Quality)
        )

        return total_c12_added

    def calculate_total_c2(dataproduct_df,aggregated_scores_df):
        sum_C2_Ownership = dataproduct_df.agg({"C2_Ownership":"sum"}).collect()[0]
        result_C2_Ownership = int(sum_C2_Ownership["sum(C2_Ownership)"])
    
        total_c2_added = aggregated_scores_df.withColumn(
        "C2_Ownership", lit(result_C2_Ownership)
        )

        return total_c2_added

    def calculate_total_c3(dataproduct_df,aggregated_scores_df):
        sum_C3_AuhoritativeSource = dataproduct_df.agg({"C3_AuhoritativeSource":"sum"}).collect()[0]
        result_C3_AuhoritativeSource = int(sum_C3_AuhoritativeSource["sum(C3_AuhoritativeSource)"])
    
        total_c3_added = aggregated_scores_df.withColumn(
        "C3_AuhoritativeSource", lit(result_C3_AuhoritativeSource)
        )

        return total_c3_added

    def calculate_total_c5(dataproduct_df,aggregated_scores_df):
        sum_C5_Catalog = dataproduct_df.agg({"C5_Catalog":"sum"}).collect()[0]
        result_C5_Catalog = int(sum_C5_Catalog["sum(C5_Catalog)"])
    
        total_c5_added = aggregated_scores_df.withColumn(
        "C5_Catalog", lit(result_C5_Catalog)
        )

        return total_c5_added

    def calculate_total_c7(dataproduct_df,aggregated_scores_df):
        sum_C7_Access = dataproduct_df.agg({"C7_Access":"sum"}).collect()[0]
        result_C7_Access = int(sum_C7_Access["sum(C7_Access)"])
    
        total_c7_added = aggregated_scores_df.withColumn(
        "C7_Access", lit(result_C7_Access)
        )

        return total_c7_added

    def calculate_total_c6(dataproduct_df,aggregated_scores_df):
        sum_C6_Classification = dataproduct_df.agg({"C6_Classification":"sum"}).collect()[0]
        result_C6_Classification = int(sum_C6_Classification["sum(C6_Classification)"])
    
        total_c6_added = aggregated_scores_df.withColumn(
        "C6_Classification", lit(result_C6_Classification)
        )

        return total_c6_added

    def calculate_total_c8(dataproduct_df,aggregated_scores_df):
        sum_C8_DataConsumptionPurpose = dataproduct_df.agg({"C8_DataConsumptionPurpose":"sum"}).collect()[0]
        result_C8_DataConsumptionPurpose = int(sum_C8_DataConsumptionPurpose["sum(C8_DataConsumptionPurpose)"])
    
        total_c8_added = aggregated_scores_df.withColumn(
        "C8_DataConsumptionPurpose", lit(result_C8_DataConsumptionPurpose)
        )

        return total_c8_added

    def calculate_actual_total_score(aggregated_scores_df):
        
        total_score = aggregated_scores_df.first()['DataHealth']+aggregated_scores_df.first()['MetadataCompleteness'] + aggregated_scores_df.first()['Use'] + aggregated_scores_df.first()['Quality']
        
        actual_total_score_added = aggregated_scores_df.withColumn(
        "ActualValue", lit(total_score)
        )

        return actual_total_score_added
    
    def calculate_aggregated_health(aggregated_scores_df):
        aggregated_health_added = aggregated_scores_df.withColumn(
        "DataHealth", lit(col("DataHealth")/col("TotalDataProducts"))
        )

        return aggregated_health_added

    def calculate_aggregated_use(aggregated_scores_df):
        aggregated_use_added = aggregated_scores_df.withColumn(
        "Use", lit(col("Use")/col("TotalDataProducts"))
        )

        return aggregated_use_added

    def calculate_aggregated_metadatacompleteness(aggregated_scores_df):
        #sum_MetadataCompleteness = final_aggregated_domain_df.agg({"MetadataCompleteness":"sum"}).collect()[0]
        #result_MetadataCompleteness= int(sum_MetadataCompleteness["sum(MetadataCompleteness)"])
        #average = result_MetadataCompleteness/totalnumberofDataProducts
        aggregated_metadatacompleteness_added = aggregated_scores_df.withColumn(
        "MetadataCompleteness", lit(col("MetadataCompleteness")/col("TotalDataProducts"))
        )

        return aggregated_metadatacompleteness_added

    def calculate_aggregated_quality(aggregated_scores_df):
        aggregated_quality_added = aggregated_scores_df.withColumn(
        "Quality", lit(col("Quality")/col("TotalDataProducts"))
        )

        return aggregated_quality_added
    
    def calculate_total_data_products_count(final_aggregated_domain_df, aggregated_scores_df):
        sum_TotalDataProducts = final_aggregated_domain_df.agg({"TotalDataProducts":"sum"}).collect()[0]
        result_TotalDataProducts = int(sum_TotalDataProducts["sum(TotalDataProducts)"])
        aggregated_total_data_products_added = aggregated_scores_df.withColumn(
        "TotalDataProducts", lit(result_TotalDataProducts)
        )

        return aggregated_total_data_products_added
    
    #Quality
    def calculate_quality_measure(dataproduct_df):
        quality_score_added = dataproduct_df.withColumn(
        "Quality", lit(((col("C12_Quality")+col("C3_AuhoritativeSource"))/2)/col("TotalDataProducts"))
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
            "DataHealth", lit((col("C12_Quality")+col("C2_Ownership")+col("C3_AuhoritativeSource")+col("C5_Catalog")+col("C7_Access")+
                                col("C8_DataConsumptionPurpose")+col("C6_Classification"))/(col("TotalDataProducts")*7)*100)
                        )

        return data_health_score_added
    
    def get_aggregation_by_businessdomain_id(dataproduct_df):
        dataproduct_df = ScoreTransformations.calculate_data_health_score(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_metadata_completeness(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_use(dataproduct_df)
        dataproduct_df = ScoreTransformations.calculate_quality_measure(dataproduct_df)

        return dataproduct_df
    
    def calculate_column_sum_by_domain(dataproduct_df):
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
        return dataproduct_df

    def calculate_aggregated_column_sum(final_aggregated_domain_df):
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
        return aggregated_scores_df
