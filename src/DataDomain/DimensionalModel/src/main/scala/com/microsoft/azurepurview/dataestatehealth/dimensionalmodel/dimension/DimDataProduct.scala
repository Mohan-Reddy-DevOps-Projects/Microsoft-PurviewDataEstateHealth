package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{DeltaTableProcessingCheck, GenerateId, SCDType2Processor, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType}

import java.util.UUID
class DimDataProduct (spark: SparkSession, logger:Logger){
  def processEmptyDimDataProduct(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDataProductNASchema = StructType(
        Array(
          StructField("DataProductDisplayName", StringType, nullable = false),
          StructField("DataProductStatusDisplayName", StringType, nullable = true),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false),
          StructField("IsActive", BooleanType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()),true)
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataProductNASchema)
      naDF = naDF.withColumn("DataProductSourceId", col("DataProductDisplayName"))

      var dfProcess = naDF
      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataProductSourceId"),"DataProductId")

      dfProcess = dfProcess.select(col("DataProductId").cast(StringType)
        ,col("DataProductSourceId").cast(StringType)
        ,col("DataProductDisplayName").cast(StringType)
        ,col("DataProductStatusDisplayName").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("ModifiedDatetime").cast(TimestampType)
        ,col("IsActive").cast(BooleanType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimDataProduct Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDataProduct Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDataProduct(df:DataFrame,schema: org.apache.spark.sql.types.StructType,AdlsTargetDirectory:String):DataFrame= {
    try {
      //Get DataProductStatus Table
      val DataProductStatusDeltaTable = DeltaTable.forPath(AdlsTargetDirectory.concat("/DomainModel/DataProductStatus"))
      val dfDataProductStatus = DataProductStatusDeltaTable.toDF

      val joinCondition = s"dataProduct.DataProductStatusID = dataProductStatus.DataProductStatusID"
      var dfProcess = df.alias("dataProduct")
        .join(dfDataProductStatus.alias("dataProductStatus"), expr(joinCondition), "left_outer")

      dfProcess  = dfProcess.select(
        col("dataProduct.DataProductID").alias("DataProductSourceId"),
        col("dataProduct.DataProductDisplayName").alias("DataProductDisplayName"),
        col("dataProductStatus.DataProductStatusDisplayName").alias("DataProductStatusDisplayName"),
        col("dataProduct.CreatedDatetime").alias("CreatedDatetime"),
        col("dataProduct.ModifiedDatetime").alias("ModifiedDatetime"),
        col("dataProduct.ExpiredFlag").alias("ExpiredFlag")
      )
      dfProcess = dfProcess.withColumn("IsActive", when(col("ExpiredFlag") === 0, true).otherwise(false))

      dfProcess = dfProcess.select(col("DataProductSourceId").cast(StringType)
        ,col("DataProductDisplayName").cast(StringType)
        ,col("DataProductStatusDisplayName").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("ModifiedDatetime").cast(TimestampType)
        ,col("IsActive").cast(BooleanType))

      //Adding NOT AVAILABLE Value Option
      val dimDataProductNASchema = StructType(
        Array(
          StructField("DataProductDisplayName", StringType, nullable = false),
          StructField("DataProductStatusDisplayName", StringType, nullable = true),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false),
          StructField("IsActive", BooleanType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()),true)
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDataProductNASchema)
      naDF = naDF.withColumn("DataProductSourceId", col("DataProductDisplayName"))

      dfProcess = dfProcess.union(naDF.select(
        col("DataProductSourceId"),
        col("DataProductDisplayName"),
        col("DataProductStatusDisplayName"),
        col("CreatedDatetime"),
        col("ModifiedDatetime"),
        col("IsActive")
      ))

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataProductSourceId"),"DataProductId")

      dfProcess = dfProcess.select(col("DataProductId").cast(StringType)
        ,col("DataProductSourceId").cast(StringType)
        ,col("DataProductDisplayName").cast(StringType)
        ,col("DataProductStatusDisplayName").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("ModifiedDatetime").cast(TimestampType)
        ,col("IsActive").cast(BooleanType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataProductSourceId is null
                            | or DataProductDisplayName is null
                            | or CreatedDatetime is null
                            | or ModifiedDatetime is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed
    } catch {
      case e: Exception =>
        println(s"Error Processing DimDataProduct Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDataProduct Dimension: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DimDataProduct"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
          if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))) {
            val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
            val dfTarget = dfTargetDeltaTable.toDF
            if (!df.isEmpty && !dfTarget.isEmpty) {

                dfTargetDeltaTable.as("target")
                  .merge(
                    df.as("source"),
                    """target.DataProductId = source.DataProductId
                      |AND target.DataProductSourceId = source.DataProductSourceId""".stripMargin)
                  .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
                  .updateAll()
                  .whenNotMatched()
                  .insertAll()
                  .execute()
            }
            else if(!df.isEmpty && dfTarget.isEmpty){
              println("Delta Table DimDataProduct Is Empty At Lake For incremental merge. Performing Full Overwrite...")
              df.write
                .format("delta")
                .mode("overwrite")
                .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
            }
          }
          else{
            println("Delta Table DimDataProduct Does not exist. Performing Full Overwrite...")
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
        println(s"Error Writing/Merging DimDataProduct data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DimDataProduct data: ${e.getMessage}")
        throw e
    }
  }
}