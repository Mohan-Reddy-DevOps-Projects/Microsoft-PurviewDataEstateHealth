package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{DeltaTableProcessingCheck, GenerateId, SCDType2Processor, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType, TimestampType}

import java.util.UUID
class DimDataAsset (spark: SparkSession, logger:Logger){
  def processEmptyDimDataAsset(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDataAssetNASchema = StructType(
        Array(
          StructField("DataAssetDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataAssetNASchema)
      naDF = naDF.withColumn("DataAssetSourceId", col("DataAssetDisplayName"))

      var dfProcess = naDF
      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("DataAssetSourceId"), "DataAssetId")

      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("DataAssetId").cast(StringType)
        , col("DataAssetSourceId").cast(StringType)
        , col("DataAssetDisplayName").cast(StringType)
        , col("CreatedDatetime").cast(TimestampType)
        , col("ModifiedDatetime").cast(TimestampType)
        , col("EffectiveDateTime").cast(TimestampType)
        , col("ExpiredDateTime").cast(TimestampType)
        , lit(1).cast(IntegerType).as("CurrentIndicator"))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimDataAsset Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDataAsset Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDataAsset(df: DataFrame, schema: org.apache.spark.sql.types.StructType, targetAdlsFullPath: String): DataFrame = {
    try {
      var dfProcess = df.select(col("DataAssetId").alias("DataAssetSourceId")
        , col("AssetDisplayName").alias("DataAssetDisplayName")
        , col("CreatedDatetime").alias("CreatedDatetime")
        , col("ModifiedDatetime").alias("ModifiedDatetime")
      )
      //Adding NOT AVAILABLE Value Option
      val dimDataAssetNASchema = StructType(
        Array(
          StructField("DataAssetDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataAssetNASchema)
      naDF = naDF.withColumn("DataAssetSourceId", col("DataAssetDisplayName"))

      val targetDeltaTableExists = DeltaTable.isDeltaTable(targetAdlsFullPath)
      // If table not exists - Or is Empty
      if (targetDeltaTableExists){
        if (spark.read.format("delta").load(targetAdlsFullPath).isEmpty) {
          dfProcess = dfProcess.union(naDF.select(
            col("DataAssetSourceId"),
            col("DataAssetDisplayName"),
            col("CreatedDatetime"),
            col("ModifiedDatetime")
          ))}
      }
      else if (!targetDeltaTableExists){
        dfProcess = dfProcess.union(naDF.select(
          col("DataAssetSourceId"),
          col("DataAssetDisplayName"),
          col("CreatedDatetime"),
          col("ModifiedDatetime")
        ))
      }

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("DataAssetSourceId"), "DataAssetId")

      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("DataAssetId").cast(StringType)
        , col("DataAssetSourceId").cast(StringType)
        , col("DataAssetDisplayName").cast(StringType)
        , col("CreatedDatetime").cast(TimestampType)
        , col("ModifiedDatetime").cast(TimestampType)
        , col("EffectiveDateTime").cast(TimestampType)
        , col("ExpiredDateTime").cast(TimestampType)
        , lit(1).cast(IntegerType).as("CurrentIndicator"))

      val filterString =
        s"""DataAssetSourceId is null
           | or DataAssetDisplayName is null
           | or CreatedDatetime is null
           | or ModifiedDatetime is null""".stripMargin
      val validator = new Validator()
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      validator.validateDataFrame(dfProcess, filterString)
      dfProcessed

      //Perform SCD2 Processing
      //val scdtype2processor = new SCDType2Processor(spark, logger)
      //scdtype2processor.processSCDType2Dimension(dfProcess,targetAdlsFullPath,"DataAssetSourceId","DataAsset",schema)

    } catch {
      case e: Exception =>
        println(s"Error Processing DimDataAsset Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDataAsset Dimension: ${e.getMessage}")
        throw e
    }
  }

  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DimDataAsset"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {
            dfTargetDeltaTable.as("target")
              .merge(
                df.as("source"),
                """target.DataAssetId = source.DataAssetId
                  |AND target.DataAssetSourceId = source.DataAssetSourceId""".stripMargin)
              .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
              .updateAll()
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DimDataAsset Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DimDataAsset Does not exist. Performing Full Overwrite...")
          val dfWrite = df
          dfWrite.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
        }
      }
    }
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DimDataAsset data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DimDataAsset data: ${e.getMessage}")
        throw e
    }
  }

}