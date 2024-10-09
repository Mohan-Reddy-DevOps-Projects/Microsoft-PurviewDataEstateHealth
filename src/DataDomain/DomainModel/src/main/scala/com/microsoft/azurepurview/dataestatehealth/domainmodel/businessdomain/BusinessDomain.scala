package com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, _}
import org.apache.spark.sql.types.{BooleanType, LongType, StringType, TimestampType}
import io.delta.tables.DeltaTable
import org.apache.spark.sql.expressions.Window

class BusinessDomain (spark: SparkSession, logger:Logger) {
  def processBusinessDomain(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
    val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
      ,col("operationType")
      ,col("_ts").alias("EventProcessingTime")
      ,col("payload.after.id").alias("BusinessDomainId")
      ,col("payload.after.name").alias("BusinessDomainName")
      ,col("payload.after.status").alias("status")
      ,col("payload.after.description").alias("description")
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
          ,col("payload.before.id").alias("BusinessDomainId")
          ,col("payload.before.name").alias("BusinessDomainName")
          ,col("payload.before.status").alias("status")
          ,col("payload.before.description").alias("description")
          ,col("payload.before.systemData.createdBy").alias("createdBy")
          ,col("payload.before.systemData.createdAt").alias("createdAt")
          ,col("payload.before.systemData.lastModifiedBy").alias("lastModifiedBy")
          ,col("payload.before.systemData.lastModifiedAt").alias("lastModifiedAt")).filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }
    dfProcess = dfProcess
      .withColumn("ParentBusinessDomainId", lit(null: StringType))
      .withColumn("IsRootDomain", lit(false))
      .withColumn("HasValidOwner", lit(true))

      dfProcess = dfProcess.filter(s"""BusinessDomainId IS NOT NULL
                                      | AND BusinessDomainName IS NOT NULL""".stripMargin).distinct()

    dfProcess = dfProcess.select(col("BusinessDomainId")
      ,col("ParentBusinessDomainId")
      ,col("BusinessDomainName").alias("BusinessDomainName")
      ,col("BusinessDomainName").alias("BusinessDomainDisplayName")
      ,col("description").alias("BusinessDomainDescription")
      ,col("status").alias("Status")
      ,col("IsRootDomain").alias("IsRootDomain")
      ,col("HasValidOwner").alias("HasValidOwner")
      ,col("AccountId").alias("AccountId")
      ,col("createdAt").alias("CreatedDatetime").cast(TimestampType)
      ,col("createdBy").alias("CreatedByUserId")
      ,col("lastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
      ,col("lastModifiedBy").alias("ModifiedByUserId")
      ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
      ,col("operationType").alias("OperationType")
    )
      val windowSpec = Window.partitionBy("BusinessDomainId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()
    val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,"BusinessDomainId is null or BusinessDomainName is null or AccountId is null")
      //validateDataFrame(dfProcessed)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing BusinessDomain Data: ${e.getMessage}")
        logger.error(s"Error Processing BusinessDomain Data: ${e.getMessage}")
        throw e
    }
  }
}
