
from pyspark.sql import SparkSession
import sys

if __name__ == "__main__":


    #create spark session
    spark = SparkSession \
    .builder \
    .appName("Data Quality Spark Job") \
    .config("spark.some.config.option", "some-value") \
    .getOrCreate()
    
    path=sys.argv[1]
    spark.sparkContext.addPyFile(path)
    
     
    from pyspark.sql import SparkSession
    from pyspark.sql.types import *
    from pyspark.sql.functions import *
    from DataEstateHealthLibrary.DataQuality.DataQualityScore.build_data_quality_scores import BuildDataQualityScores
    from DataEstateHealthLibrary.DataQuality.DataQualityScore.data_quality_sink_schema import DataQualitySinkSchema


    print(sys.argv[1])
    print(sys.argv[2])
    print(sys.argv[3])
    print(sys.argv[4])
    print(sys.argv[5])
    
    source = sys.argv[2]
    sink = sys.argv[3]
    dataqualityfactdeleted = source+sys.argv[4]+sys.argv[5]
    dataqualitysink = sink+sys.argv[4]
    
    #FETCH DELETED DATA QUALITY FACT
    try:
        deleted_dataqualityfact_df = spark.read.option('startingVersion', "latest").load(dataqualityfactdeleted+'DataQualityFact', format='delta')
    except Exception as error:
        deleted_dataqualityfact_df = spark.createDataFrame([], StructType([]))
       
    print("fetched_deleted")

    #DATA QUALITY SCORE SOURCE FROM SINK
    try:
        sink_dataqualityscore_df = spark.read.option('startingVersion', "latest").load(dataqualitysink+'DataQualityScore', format='delta')
    except Exception as error:
        sink_dataqualityscore_df = spark.createDataFrame([], StructType([]))
        
    print("fetched_source")

    #GENERATE DATA QUALITY SCORES
    if not sink_dataqualityscore_df.isEmpty():
        #HANDLE DATA QUALITY SCORE DELETES
        sink_dataqualityscore_df = BuildDataQualityScores.handle_dataquality_deletes(sink_dataqualityscore_df,deleted_dataqualityfact_df)
        
        #Generate productquality_score_df
        productquality_score_df = BuildDataQualityScores.build_product_score(sink_dataqualityscore_df)
        sink_productquality_score_df = spark.createDataFrame(productquality_score_df.rdd, schema=DataQualitySinkSchema.sink_product_dataqualityscore_schema)
        sink_productquality_score_df.write.format("delta").mode("overwrite").save(sink+"ProductQualityScore")
    
        #Generate domainquality_score_df
        domainquality_score_df = BuildDataQualityScores.build_domain_score(sink_dataqualityscore_df)
        sink_domainquality_score_df = spark.createDataFrame(domainquality_score_df.rdd, schema=DataQualitySinkSchema.sink_domain_dataqualityscore_schema)
        sink_domainquality_score_df.write.format("delta").mode("overwrite").save(sink+"DomainQualityScore")
    
        #Generate assetquality_score_df
        assetquality_score_df = BuildDataQualityScores.build_asset_score(sink_dataqualityscore_df)
        sink_assetquality_score_df = spark.createDataFrame(assetquality_score_df.rdd, schema=DataQualitySinkSchema.sink_asset_dataqualityscore_schema)
        sink_assetquality_score_df.write.format("delta").mode("overwrite").save(sink+"AssetQualityScore")
        
        print("GENERATED DATA QUALITY SCORES")
    else:
        sink_productquality_score_df = spark.createDataFrame([], DataQualitySinkSchema.sink_product_dataqualityscore_schema)
        sink_productquality_score_df.write.format("delta").mode("overwrite").save(sink+"ProductQualityScore")
        
        sink_domainquality_score_df = spark.createDataFrame([], DataQualitySinkSchema.sink_domain_dataqualityscore_schema)
        sink_domainquality_score_df.write.format("delta").mode("overwrite").save(sink+"DomainQualityScore")
        
        sink_assetquality_score_df = spark.createDataFrame([], DataQualitySinkSchema.sink_asset_dataqualityscore_schema)
        sink_assetquality_score_df.write.format("delta").mode("overwrite").save(sink+"AssetQualityScore")
        
        print("GENERATED EMPTY DATA QUALITY SCORES SINK")
