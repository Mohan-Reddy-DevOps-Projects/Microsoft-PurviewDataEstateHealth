package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, _}
import org.apache.spark.sql.types.{BooleanType, LongType, StringType, TimestampType}
class DataAssetTypeDataType (spark: SparkSession, logger:Logger){
  def processDataAssetTypeDataType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("DataTypeId").cast(StringType)
        ,col("DataAssetTypeId").cast(StringType)
        ,col("ColumnDataType").alias("DataTypeDisplayName").cast(StringType)).distinct()

      dfProcess = dfProcess.filter(s"""DataTypeId IS NOT NULL
                                      | AND DataAssetTypeId IS NOT NULL
                                      | AND DataTypeDisplayName IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetTypeDataType Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetTypeDataType Data: ${e.getMessage}")
        throw e
    }
  }
}
