from pyspark.sql.functions import *

class CdmcControlsTransformations:
    
    #Ownership_control2
    def calculate_cdmc_control_2(dataproduct_df):
        return dataproduct_df.select("DataProductId","HasValidOwner").filter(col("HasValidOwner") == True)

    #IsAuthoritativeSource_control3
    def calculate_cdmc_control_3(dataproduct_df):
        return dataproduct_df.select("DataProductId","IsAuthoritativeSource").filter(col("IsAuthoritativeSource") == True)

    #Catalog_control5
    def calculate_cdmc_control_5(dataproduct_df):
        dataproduct_df = dataproduct_df.withColumn("TotalNumberOfDataProducts", lit(count))
        dataproduct_df = dataproduct_df.filter(col("HasValidOwner") == True)
        dataproduct_df = dataproduct_df.filter(col("HasNotNullDescription") == True)
        dataproduct_df = dataproduct_df.filter(col("HasValidUseCase") == True)
        dataproduct_df = dataproduct_df.filter(col("AssetCount") > 0)
        dataproduct_df = dataproduct_df.filter(col("GlossaryTermCount") > 0)
        dataproduct_df = dataproduct_df.filter(col("HasAccessEntitlement") == True)
        return dataproduct_df.select("DataProductId","HasValidOwner", "HasValidUseCase", "HasNotNullDescription", "GlossaryTermCount", "HasAccessEntitlement", "AssetCount")

    #ClassificationPassCount_control_6
    def calculate_cdmc_control_6(dataproduct_df):
        return dataproduct_df.select("DataProductId","ClassificationPassCount").filter(col("ClassificationPassCount") >= 2)

    #access_entitlement_control7
    def calculate_cdmc_control_7(dataproduct_df):
        return dataproduct_df.select("DataProductId","HasAccessEntitlement").filter(col("HasAccessEntitlement") == True)

    #data_consumption_purpose_control_8
    def calculate_cdmc_control_8(dataproduct_df):
        return dataproduct_df.select("DataProductId","HasDataShareAgreementSetOrExempt").filter(col("HasDataShareAgreementSetOrExempt") == True)
    
    #HasDataQualityScore_control_12
    def calculate_cdmc_control_12(dataproduct_df):
        return dataproduct_df.select("DataProductId","HasDataQualityScore").filter(col("HasDataQualityScore") == True)

    #Quality Measure
    def calculate_quality_measure(data_products_total_count,cdmc_control_3_count, cdmc_control_12_count):
        quality_and_authority_average = (cdmc_control_3_count + cdmc_control_12_count)/2
        quality_measure = quality_and_authority_average/data_products_total_count
        return quality_measure

    #Use Measure
    def calculate_use_measure(data_products_total_count,cdmc_control_7_count, cdmc_control_8_count):
        access_entitlement_and_data_consumption_average = (cdmc_control_7_count + cdmc_control_8_count)/2
        use_measure = access_entitlement_and_data_consumption_average/data_products_total_count
        return use_measure

    #Metadata completeness
    def calculate_metadata_completeness(data_products_total_count,cdmc_control_2_count, cdmc_control_5_count, cdmc_control_6_count):
        ownership_catalog_classification_average = (cdmc_control_2_count + cdmc_control_5_count + cdmc_control_6_count)/3
        metadata_completeness = ownership_catalog_classification_average/data_products_total_count
        return metadata_completeness

    #Data Health score
    def calculate_data_health_score(data_products_total_count,cdmc_control_2_count, cdmc_control_3_count, cdmc_control_5_count, cdmc_control_6_count,
                                        cdmc_control_7_count, cdmc_control_8_count, cdmc_control_12_count):
        all_controls_average = (cdmc_control_2_count + cdmc_control_3_count + cdmc_control_5_count+ cdmc_control_6_count +
                                                    cdmc_control_7_count + cdmc_control_8_count + cdmc_control_12_count)/7
        data_health_score = all_controls_average/data_products_total_count
        return data_health_score

