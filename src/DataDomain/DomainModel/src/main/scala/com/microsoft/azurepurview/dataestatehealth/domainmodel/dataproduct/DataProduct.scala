package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, lit, row_number, when}
import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._

import java.sql.Timestamp

class DataProduct (spark: SparkSession, logger:Logger){
  def processDataProduct(df:DataFrame,schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Read dependent Tables DataProductStatus, DataProductUpdateFrequency, DataProductType
      val dfDataProductStatus=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/DataProductStatus"))
      val dfDataProductUpdateFrequency=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/DataProductUpdateFrequency"))
      val dfDataProductType=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/DataProductType"))

      //Start Processing DataProduct Delta Table
      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("DataProductId")
        ,col("payload.after.name").alias("DataProductDisplayName")
        ,col("payload.after.description").alias("description")
        ,col("payload.after.domain").alias("BusinessDomainId")
        ,col("payload.after.type").alias("DataProductTypeDisplayName")
        ,col("payload.after.businessUse").alias("businessUse")
        ,col("payload.after.updateFrequency").alias("UpdateFrequency")
        ,col("payload.after.sensitivityLabel").alias("SensitivityLabel")
        ,col("payload.after.endorsed").alias("Endorsed")
        ,col("payload.after.status").alias("DataProductStatus")
        ,col("payload.after.systemData.createdBy").alias("createdBy")
        ,col("payload.after.systemData.createdAt").alias("createdAt")
        ,col("payload.after.systemData.lastModifiedBy").alias("lastModifiedBy")
        ,col("payload.after.systemData.lastModifiedAt").alias("lastModifiedAt")).filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
      val dfProcessDelete = df.select(col("accountId").alias("AccountId")
        ,col("operationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.before.id").alias("DataProductId")
        ,col("payload.before.name").alias("DataProductDisplayName")
        ,col("payload.before.description").alias("description")
        ,col("payload.before.domain").alias("BusinessDomainId")
        ,col("payload.before.type").alias("DataProductTypeDisplayName")
        ,col("payload.before.businessUse").alias("businessUse")
        ,col("payload.before.updateFrequency").alias("UpdateFrequency")
        ,col("payload.before.sensitivityLabel").alias("SensitivityLabel")
        ,col("payload.before.endorsed").alias("Endorsed")
        ,col("payload.before.status").alias("DataProductStatus")
        ,col("payload.before.systemData.createdBy").alias("createdBy")
        ,col("payload.before.systemData.createdAt").alias("createdAt")
        ,col("payload.before.systemData.lastModifiedBy").alias("lastModifiedBy")
        ,col("payload.before.systemData.lastModifiedAt").alias("lastModifiedAt")).filter("operationType=='Delete'")
      dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)}
      else {
      dfProcess = dfProcessUpsert
    }
    val windowSpecDataProduct = Window.partitionBy("DataProductId")
      .orderBy(coalesce(col("lastModifiedAt").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
    dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpecDataProduct))
      .filter(col("row_number") === 1)
      .drop("row_number")
      .distinct()

      dfProcess = dfProcess
        .withColumn("ExpiredFlag", when(lower(trim(col("operationType"))) === "delete", 1).otherwise(0))
        .withColumn("ExpiredFlagLastModifiedDatetime", col("lastModifiedAt"))
        .withColumn("DataProductStatusLastUpdatedDatetime", col("lastModifiedAt"))

      var dfJoin = dfProcess
        .join(dfDataProductStatus, lower(trim(dfProcess("DataProductStatus"))) === lower(trim(dfDataProductStatus("DataProductStatusDisplayName"))))
        .join(dfDataProductUpdateFrequency, lower(trim(dfProcess("UpdateFrequency"))) === lower(trim(dfDataProductUpdateFrequency("UpdateFrequencyDisplayName"))),"left")
        .join(dfDataProductType, lower(trim(dfProcess("DataProductTypeDisplayName"))) === lower(trim(dfDataProductType("DataProductTypeDisplayName"))))
        .select("*")

      dfJoin = dfJoin.select(col("DataProductID").cast(StringType)
        ,col("DataProductDisplayName").cast(StringType)
        ,col("description").alias("DataProductDescription").cast(StringType)
        ,col("AccountId").alias("AccountId").cast(StringType)
        ,col("DataProductTypeID").alias("DataProductTypeID").cast(StringType)
        ,col("businessUse").alias("UseCases").cast(StringType)
        ,col("SensitivityLabel").alias("SensitivityLabel").cast(StringType)
        ,col("Endorsed").alias("Endorsed").cast(BooleanType)
        ,col("ExpiredFlag").alias("ExpiredFlag").cast(IntegerType)
        ,col("ExpiredFlagLastModifiedDatetime").alias("ExpiredFlagLastModifiedDatetime").cast(TimestampType)
        ,col("DataProductStatusID").alias("DataProductStatusID").cast(StringType)
        ,col("DataProductStatusLastUpdatedDatetime").alias("DataProductStatusLastUpdatedDatetime").cast(TimestampType)
        ,col("UpdateFrequencyId").alias("UpdateFrequencyId").cast(StringType)
        ,col("createdAt").alias("CreatedDatetime").cast(TimestampType)
        ,col("createdBy").alias("CreatedByUserId").cast(StringType)
        ,col("lastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        ,col("lastModifiedBy").alias("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
        ,col("operationType").alias("OperationType").cast(StringType)
        ,col("BusinessDomainId").alias("BusinessDomainId").cast(StringType)
      )

      dfJoin = dfJoin.filter(s"""DataProductId IS NOT NULL
                                      | AND DataProductStatusID IS NOT NULL
                                      | AND DataProductTypeID IS NOT NULL
                                      | AND CreatedByUserId IS NOT NULL
                                      | AND ModifiedByUserId IS NOT NULL""".stripMargin).distinct()

      val windowSpec = Window.partitionBy("DataProductID").orderBy(col("ModifiedDateTime").desc)
      dfJoin = dfJoin.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()
      val dfProcessed = spark.createDataFrame(dfJoin.rdd, schema=schema)
      val filterString = s"""DataProductId is null
                            | or DataProductStatusID is null
                            | or DataProductTypeID is null
                            | or CreatedByUserId is null
                            | or ModifiedByUserId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProduct Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProduct Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataProduct"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)){
      val dfWrite = df.drop("BusinessDomainId")
      if (refreshType=="full"){
        dfWrite.write
          .format("delta")
          .mode("overwrite")
          .save(adlsTargetDirectory.concat("/").concat(EntityName))}
      else if (refreshType=="incremental") {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {

            val maxEventProcessingTime = dfTarget.agg(max("EventProcessingTime")
              .as("maxEventProcessingTime"))
              .collect()(0)
              .getLong(0)

            val mergeDfSource = dfWrite
              .filter(col("EventProcessingTime") > maxEventProcessingTime)

            dfTargetDeltaTable.as("target")
              .merge(
                mergeDfSource.as("source"),
                """target.DataProductID = source.DataProductID""")
              .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
              .updateAll()
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataProduct Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            dfWrite.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataProduct Does not exist for incremental merge. Performing Full Overwrite...")
          dfWrite.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataProduct data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataProduct data: ${e.getMessage}")
        throw e
    }
  }
}
