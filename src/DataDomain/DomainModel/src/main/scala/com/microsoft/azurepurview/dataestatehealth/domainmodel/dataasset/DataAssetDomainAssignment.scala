package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, lit, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession, types}

import java.sql.Timestamp
class DataAssetDomainAssignment (spark: SparkSession, logger:Logger){

  def processDataAssetDomainAssignment(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      //Frame DataAsset Dataframe
      var dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("DataAssetId")
        ,col("payload.after.domain").alias("BusinessDomainId")
        ,col("payload.after.systemData.lastModifiedBy").alias("AssignedByUserId")
        ,col("payload.after.systemData.lastModifiedAt").alias("ActiveFlagLastModifiedDatetime")
        ,col("payload.after.systemData.lastModifiedAt").alias("AssignmentLastModifiedDatetime")
        ,lit(1).alias("ActiveFlag")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
        ,col("payload.after.systemData.lastModifiedBy").alias("ModifiedByUserId"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        var dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.id").alias("DataAssetId")
          ,col("payload.before.domain").alias("BusinessDomainId")
          ,col("payload.before.systemData.lastModifiedBy").alias("AssignedByUserId")
          ,col("payload.before.systemData.lastModifiedAt").alias("ActiveFlagLastModifiedDatetime")
          ,col("payload.before.systemData.lastModifiedAt").alias("AssignmentLastModifiedDatetime")
          ,lit(0).alias("ActiveFlag")
          ,col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime")
          ,col("payload.before.systemData.lastModifiedBy").alias("ModifiedByUserId"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      val windowSpec = Window.partitionBy("DataAssetId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess.filter(s"""DataAssetId IS NOT NULL
                                      | AND BusinessDomainId IS NOT NULL
                                      | AND AssignedByUserId IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("DataAssetId").cast(StringType)
        ,col("BusinessDomainId").cast(StringType)
        ,col("AssignedByUserId").alias("AssignedByUserId").cast(StringType)
        ,col("ActiveFlagLastModifiedDatetime").alias("ActiveFlagLastModifiedDatetime").cast(TimestampType)
        ,col("AssignmentLastModifiedDatetime").alias("AssignmentLastModifiedDatetime").cast(TimestampType)
        ,col("ActiveFlag").alias("ActiveFlag").cast(IntegerType)
        ,col("ModifiedDateTime").alias("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").alias("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
        ,col("OperationType").alias("OperationType").cast(StringType)
      ).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataAssetId is null
                            | or BusinessDomainId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetDomainAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetDomainAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
