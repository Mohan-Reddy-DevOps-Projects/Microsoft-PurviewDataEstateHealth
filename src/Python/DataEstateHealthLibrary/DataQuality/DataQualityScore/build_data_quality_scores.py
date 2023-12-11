from DataEstateHealthLibrary.DataQuality.DataQualityFact.data_quality_fact_column_functions import DataQualityFactColumnFunctions
from DataEstateHealthLibrary.DataQuality.DataQualityFact.data_quality_fact_transformations import DataQualityFactTransformations
from DataEstateHealthLibrary.DataQuality.DataQualityScore.data_quality_score_transformations import DataQualityScoreTransformations
from pyspark.sql.functions import *
from pyspark.sql.types import *
from DataEstateHealthLibrary.Shared.helper_function import HelperFunction

class BuildDataQualityScores:
    def build_product_score(dataqualityscore_df):
        
        dataqualityscore_df = dataqualityscore_df.select("DataProductId","JobId", "QualityScore", "ResultedAt")

        productquality_score_df = DataQualityScoreTransformations.dedup_quality_score(dataqualityscore_df)
        
        productquality_score_df = productquality_score_df.withColumn("QualityScore",productquality_score_df['QualityScore'].cast(DoubleType()))
        productquality_score_df = DataQualityScoreTransformations.calculate_total_count_col(productquality_score_df)
        productquality_score_df = DataQualityScoreTransformations.calculate_column_sum(productquality_score_df, "DataProductId")
        productquality_score_df = DataQualityScoreTransformations.calculate_score(productquality_score_df)
        productquality_score_df = HelperFunction.calculate_last_refreshed_at(productquality_score_df,"LastRefreshedAt")
        productquality_score_df = productquality_score_df.select("DataProductId","QualityScore","LastRefreshedAt")
        return productquality_score_df

    def build_domain_score(dataqualityscore_df):
        
        dataqualityscore_df = dataqualityscore_df.select("BusinessDomainId","JobId", "QualityScore", "ResultedAt")
        
        domainquality_score_df = DataQualityScoreTransformations.dedup_quality_score(dataqualityscore_df)
        domainquality_score_df = domainquality_score_df.withColumn("QualityScore",domainquality_score_df['QualityScore'].cast(DoubleType()))
        domainquality_score_df = DataQualityScoreTransformations.calculate_total_count_col(domainquality_score_df)
        domainquality_score_df = DataQualityScoreTransformations.calculate_column_sum(domainquality_score_df, "BusinessDomainId")
        domainquality_score_df = DataQualityScoreTransformations.calculate_score(domainquality_score_df)
        domainquality_score_df = HelperFunction.calculate_last_refreshed_at(domainquality_score_df,"LastRefreshedAt")
        domainquality_score_df = domainquality_score_df.select("BusinessDomainId","QualityScore","LastRefreshedAt")
        
        return domainquality_score_df

    def build_asset_score(dataqualityscore_df):
        
        dataqualityscore_df = dataqualityscore_df.select("DataAssetId","JobId", "QualityScore", "ResultedAt")

        assetquality_score_df = DataQualityScoreTransformations.dedup_quality_score(dataqualityscore_df)

        #need to explicitly cast it since we get it as string from source. we cannot calculate sum on string columns types.
        assetquality_score_df = assetquality_score_df.withColumn("QualityScore",assetquality_score_df['QualityScore'].cast(DoubleType()))
        assetquality_score_df = DataQualityScoreTransformations.calculate_total_count_col(assetquality_score_df)
        assetquality_score_df = DataQualityScoreTransformations.calculate_column_sum(assetquality_score_df, "DataAssetId")
        assetquality_score_df = DataQualityScoreTransformations.calculate_score(assetquality_score_df)
        assetquality_score_df = HelperFunction.calculate_last_refreshed_at(assetquality_score_df,"LastRefreshedAt")
        assetquality_score_df = assetquality_score_df.select("DataAssetId","QualityScore","LastRefreshedAt")
        
        return assetquality_score_df
        
    def handle_dataquality_deletes(dataqualityscore_df,deleted_dataqualityfact_df):

        if not deleted_dataqualityfact_df.isEmpty():
            deleted_dataqualityfact_df = DataQualityFactColumnFunctions.add_id_schema(deleted_dataqualityfact_df)
            deleted_dataqualityfact_df = DataQualityFactTransformations.add_job_id(deleted_dataqualityfact_df)
            deleted_dataqualityfact_df = deleted_dataqualityfact_df.select("JobId");
            dataqualityscore_df = dataqualityscore_df.join(deleted_dataqualityfact_df, ["JobId"], "leftanti")
            
        return dataqualityscore_df
