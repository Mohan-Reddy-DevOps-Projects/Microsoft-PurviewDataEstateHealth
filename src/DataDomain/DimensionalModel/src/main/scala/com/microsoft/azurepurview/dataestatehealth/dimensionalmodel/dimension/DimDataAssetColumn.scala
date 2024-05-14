package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{DeltaTableProcessingCheck, GenerateId, SCDType2Processor, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType, TimestampType}

import java.util.UUID
class DimDataAssetColumn (spark: SparkSession, logger:Logger){
  def processEmptyDimDataAssetColumn(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDataAssetColumnNASchema = StructType(
        Array(
          StructField("DataAssetColumnDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataAssetColumnNASchema)
      naDF = naDF.withColumn("DataAssetColumnSourceId", sha2(col("DataAssetColumnDisplayName"), 256))
      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DataAssetColumnSourceId"), "DataAssetColumnSourceId")

      var dfProcess = naDF
      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("DataAssetColumnSourceId"), "DataAssetColumnId")

      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("DataAssetColumnId").cast(StringType)
        , col("DataAssetColumnSourceId").cast(StringType)
        , col("DataAssetColumnDisplayName").cast(StringType)
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
        println(s"Error Processing EmptyDimDataAssetColumn Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDataAssetColumn Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDataAssetColumn(df:DataFrame,schema: org.apache.spark.sql.types.StructType,targetAdlsFullPath:String):DataFrame= {
    try {
      var dfProcess = df
        .drop("DataAssetId")

      dfProcess = dfProcess.select(col("ColumnId").alias("DataAssetColumnSourceId")
        ,col("ColumnDisplayName").alias("DataAssetColumnDisplayName")
        ,col("CreatedDatetime").alias("CreatedDatetime")
        ,col("ModifiedDatetime").alias("ModifiedDatetime")
      )
      //Adding NOT AVAILABLE Value Option
      val dimDataAssetColumnNASchema = StructType(
        Array(
          StructField("DataAssetColumnDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataAssetColumnNASchema)
      naDF = naDF.withColumn("DataAssetColumnSourceId", sha2(col("DataAssetColumnDisplayName"), 256))
      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DataAssetColumnSourceId"), "DataAssetColumnSourceId")

      val targetDeltaTableExists = DeltaTable.isDeltaTable(targetAdlsFullPath)
      // If table not exists - Or is Empty
      if (targetDeltaTableExists){
        if (spark.read.format("delta").load(targetAdlsFullPath).isEmpty) {
          dfProcess = dfProcess.union(naDF.select(
            col("DataAssetColumnSourceId"),
            col("DataAssetColumnDisplayName"),
            col("CreatedDatetime"),
            col("ModifiedDatetime")
          ))}
      }
      else if (!targetDeltaTableExists){
        dfProcess = dfProcess.union(naDF.select(
          col("DataAssetColumnSourceId"),
          col("DataAssetColumnDisplayName"),
          col("CreatedDatetime"),
          col("ModifiedDatetime")
        ))
      }

      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("DataAssetColumnSourceId"), "DataAssetColumnId")

      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("DataAssetColumnId").cast(StringType)
        , col("DataAssetColumnSourceId").cast(StringType)
        , col("DataAssetColumnDisplayName").cast(StringType)
        , col("CreatedDatetime").cast(TimestampType)
        , col("ModifiedDatetime").cast(TimestampType)
        , col("EffectiveDateTime").cast(TimestampType)
        , col("ExpiredDateTime").cast(TimestampType)
        , lit(1).cast(IntegerType).as("CurrentIndicator"))

      val filterString = s"""DataAssetColumnSourceId is null
                            | or DataAssetColumnDisplayName is null
                            | or CreatedDatetime is null
                            | or ModifiedDatetime is null""".stripMargin

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed

      //Perform SCD2 Processing
      //val scdtype2processor = new SCDType2Processor(spark, logger)
      //scdtype2processor.processSCDType2Dimension(dfProcess,targetAdlsFullPath,"DataAssetColumnSourceId","DataAssetColumn",schema)

    } catch {
      case e: Exception =>
        println(s"Error Processing DimDataAssetColumn Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDataAssetColumn Dimension: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DimDataAssetColumn"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {
            dfTargetDeltaTable.as("target")
              .merge(
                df.as("source"),
                """target.DataAssetColumnId = source.DataAssetColumnId
                  |AND target.DataAssetColumnSourceId = source.DataAssetColumnSourceId""".stripMargin)
              .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
              .updateAll()
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataAssetColumn Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataAssetColumn Does not exist. Performing Full Overwrite...")
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
        println(s"Error Writing/Merging DimDataAssetColumn data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DimDataAssetColumn data: ${e.getMessage}")
        throw e
    }
  }
}