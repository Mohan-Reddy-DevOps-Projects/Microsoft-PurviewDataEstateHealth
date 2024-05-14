package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, lit, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession, types}
import org.apache.spark.sql.functions._
class DataProductAssetAssignment (spark: SparkSession, logger:Logger){
  def processDataProductAssetAssignment(schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Read dependent Table Relationship
      val dfRelationship=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/Relationship"))

      var dfProcess = dfRelationship.withColumn("ActiveFlag",when(lower(trim(col("OperationType"))) === "delete", 0).otherwise(1).alias("ActiveFlag"))
      dfProcess = dfProcess.select(col("SourceId").alias("DataProductId").cast(StringType)
      ,col("TargetId").alias("DataAssetId").cast(StringType)
      ,col("ModifiedByUserId").alias("AssignedByUserId").cast(StringType)
      ,col("ModifiedDateTime").alias("ActiveFlagLastModifiedDatetime").cast(TimestampType)
        ,col("ModifiedDateTime").alias("AssignmentLastModifiedDatetime").cast(TimestampType)
        ,col("ActiveFlag").cast(IntegerType)
        ,col("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType").cast(StringType))
        .filter("sourceType = 'DataProduct' AND TargetType = 'DataAsset'")
        .distinct()

      dfProcess = dfProcess.filter(s"""DataProductId IS NOT NULL
                                      | AND DataAssetId IS NOT NULL
                                      | AND AssignedByUserId IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataProductId is null
                            | or DataAssetId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductAssetAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductAssetAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
