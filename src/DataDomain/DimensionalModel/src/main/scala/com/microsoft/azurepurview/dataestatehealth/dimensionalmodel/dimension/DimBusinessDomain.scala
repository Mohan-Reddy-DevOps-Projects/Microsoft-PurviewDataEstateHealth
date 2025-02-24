package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{DeltaTableProcessingCheck, GenerateId, SCDType2Processor, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import java.util.UUID
import java.sql.{Date, Timestamp}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types._
class DimBusinessDomain (spark: SparkSession, logger:Logger){
  def processEmptyDimBusinessDomain(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimBusinessDomainNASchema = StructType(
        Array(
          StructField("BusinessDomainDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimBusinessDomainNASchema)
      naDF = naDF.withColumn("BusinessDomainSourceId", col("BusinessDomainDisplayName"))

      var dfProcess = naDF
      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("BusinessDomainSourceId"),"BusinessDomainId")
      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("BusinessDomainId").cast(StringType)
        ,col("BusinessDomainSourceId").cast(StringType)
        ,col("BusinessDomainDisplayName").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("ModifiedDatetime").cast(TimestampType)
        ,col("EffectiveDateTime").cast(TimestampType)
        ,col("ExpiredDateTime").cast(TimestampType)
        ,lit(1).cast(IntegerType).as("CurrentIndicator"))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimBusinessDomain Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimBusinessDomain Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimBusinessDomain(df:DataFrame,schema: org.apache.spark.sql.types.StructType,targetAdlsFullPath:String):DataFrame= {
    try {
      var dfProcess = df.select(col("BusinessDomainId").alias("BusinessDomainSourceId")
        ,col("BusinessDomainName").alias("BusinessDomainDisplayName")
        ,col("CreatedDatetime").alias("CreatedDatetime")
        ,col("ModifiedDatetime").alias("ModifiedDatetime")
      )

      //Adding NOT AVAILABLE Value Option
      val dimBusinessDomainNASchema = StructType(
        Array(
          StructField("BusinessDomainDisplayName", StringType, nullable = false),
          StructField("CreatedDatetime", TimestampType, nullable = false),
          StructField("ModifiedDatetime", TimestampType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE", java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()), java.sql.Timestamp.valueOf(java.time.LocalDateTime.now()))
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimBusinessDomainNASchema)
      naDF = naDF.withColumn("BusinessDomainSourceId", col("BusinessDomainDisplayName"))

      dfProcess = dfProcess.union(naDF.select(
        col("BusinessDomainSourceId"),
        col("BusinessDomainDisplayName"),
        col("CreatedDatetime"),
        col("ModifiedDatetime")
      ))

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("BusinessDomainSourceId"),"BusinessDomainId")
      dfProcess = dfProcess.withColumn("EffectiveDateTime", col("CreatedDatetime"))
        .withColumn("ExpiredDateTime", col("ModifiedDatetime"))

      dfProcess = dfProcess.select(col("BusinessDomainId").cast(StringType)
        ,col("BusinessDomainSourceId").cast(StringType)
        ,col("BusinessDomainDisplayName").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("ModifiedDatetime").cast(TimestampType)
        ,col("EffectiveDateTime").cast(TimestampType)
        ,col("ExpiredDateTime").cast(TimestampType)
        ,lit(1).cast(IntegerType).as("CurrentIndicator"))

      val filterString = s"""BusinessDomainSourceId is null
                            | or BusinessDomainDisplayName is null
                            | or CreatedDatetime is null
                            | or ModifiedDatetime is null""".stripMargin
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed
      //Perform SCD2 Processing
      //val scdtype2processor = new SCDType2Processor(spark, logger)
      //scdtype2processor.processSCDType2Dimension(dfProcess,targetAdlsFullPath,"BusinessDomainSourceId","BusinessDomain",schema)
    } catch {
      case e: Exception =>
        println(s"Error Processing DimBusinessDomain Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimBusinessDomain Dimension: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DimBusinessDomain"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {
            dfTargetDeltaTable.as("target")
              .merge(
                df.as("source"),
                """target.BusinessDomainId = source.BusinessDomainId AND
                   target.BusinessDomainSourceId = source.BusinessDomainSourceId""")
              .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
              .updateAll()
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DimBusinessDomain Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DimBusinessDomain Does not exist. Performing Full Overwrite...")
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
        println(s"Error Writing/Merging DimBusinessDomain data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DimBusinessDomain data: ${e.getMessage}")
        throw e
    }
  }
}
