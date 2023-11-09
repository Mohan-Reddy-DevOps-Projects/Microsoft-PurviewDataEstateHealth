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
        (col("HasNotNullDescription") == 1) &
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

    def add_total_data_products(dataproduct_df):
        total_data_products_added = dataproduct_df.withColumn(
        "TotalDataProducts", lit(1)
        )

        return total_data_products_added
    
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
