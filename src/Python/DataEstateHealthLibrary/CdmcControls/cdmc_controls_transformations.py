from tokenize import Double
from pyspark.sql.functions import *

class CdmcControlsTransformations:
    
    #Ownership_control2
    def calculate_cdmc_control_2(dataproduct_df):
        cdmc_control_2_added = dataproduct_df.withColumn(
            "C2_Ownership", lit(col("HasValidOwner"))
            )
        return cdmc_control_2_added
    
    #IsAuthoritativeSource_control3
    def calculate_cdmc_control_3(dataproduct_df):
        cdmc_control_3_added = dataproduct_df.withColumn(
            "C3_AuhoritativeSource", lit(col("IsAuthoritativeSource"))
            )                                                              
        return cdmc_control_3_added

    #Catalog_control5
    def calculate_cdmc_control_5(dataproduct_df):
        cdmc_control_5_added = dataproduct_df.withColumn(
        "C5_Catalog", when((col("HasValidOwner") == 1) &
        (col("HasDescription") == 1) &
        (col("HasValidUseCase") == 1) &
        (col("AssetCount") > 0) &
        (col("GlossaryTermCount") > 0) &
        (col("HasAccessEntitlement") == 1), lit(1))
        .otherwise(lit(0))
        )
        return cdmc_control_5_added

    #ClassificationPassCount_control_6
    def calculate_cdmc_control_6(dataproduct_df):
        cdmc_control_6_added = dataproduct_df.withColumn(
            "C6_Classification", when(col("ClassificationPassCount") > 0, lit(1))
            .otherwise(lit(0))
            ) 
        return cdmc_control_6_added
    
    #access_entitlement_control7
    def calculate_cdmc_control_7(dataproduct_df):
         cdmc_control_7_added = dataproduct_df.withColumn(
            "C7_Access", lit(col("HasAccessEntitlement"))
            )
         return cdmc_control_7_added

    #data_consumption_purpose_control_8
    def calculate_cdmc_control_8(dataproduct_df):
        cdmc_control_8_added = dataproduct_df.withColumn(
            "C8_DataConsumptionPurpose", lit(col("HasDataShareAgreementSetOrExempt"))
            )
        return cdmc_control_8_added
    
    #HasDataQualityScore_control_12
    def calculate_cdmc_control_12(dataproduct_df):
        cdmc_control_12_added = dataproduct_df.withColumn(
            "C12_Quality", lit(col("HasDataQualityScore"))
            )
        return cdmc_control_12_added

    def calculate_cdmc_column_sum_by_domain(dataproduct_df):
        dataproduct_df = dataproduct_df.groupBy("BusinessDomainId").sum()
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C2_Ownership)","Ownership")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C3_AuhoritativeSource)","AuhoritativeSource")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C5_Catalog)","Catalog")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C6_Classification)","Classification")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C7_Access)","Access")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C8_DataConsumptionPurpose)","DataConsumptionPurpose")
        dataproduct_df = dataproduct_df.withColumnRenamed("sum(C12_Quality)","Quality")
        return dataproduct_df
    
    def calculate_aggregated_cdmc_column_sum(final_aggregated_cdmc_control_domain_df):
        aggregated_scores_df = final_aggregated_cdmc_control_domain_df.groupBy().sum()
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Ownership)","Ownership")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(AuhoritativeSource)","AuhoritativeSource")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Catalog)","Catalog")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Classification)","Classification")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Access)","Access")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(DataConsumptionPurpose)","DataConsumptionPurpose")
        aggregated_scores_df = aggregated_scores_df.withColumnRenamed("sum(Quality)","Quality")
        return aggregated_scores_df
