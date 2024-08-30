package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, explode_outer, expr, lower, max, row_number, when}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.types._

class DataSubscriberRequest (spark: SparkSession, logger:Logger){
  def processDataSubscriberRequest(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.After.id").alias("SubscriberRequestId")
        ,col("payload.After.dataProductId").alias("DataProductId")
        ,col("payload.After.domainId").alias("BusinessDomainId")
        ,col("payload.After.appliedPolicySet.id").alias("AccessPolicySetId")
        ,col("payload.After.subscriberIdentity.identityType").alias("SubscriberIdentityTypeDisplayName")
        ,col("payload.After.requestorIdentity.identityType").alias("RequestorIdentityTypeDisplayName")
        ,col("payload.After.provisioningState").alias("SubscriberRequestStatus")
        ,col("payload.After.subscriptionStatus").alias("RequestStatusDisplayName")
        ,col("payload.After.subscriberIdentity.objectId").alias("SubscribedByUserId")
        ,col("payload.After.subscriberIdentity.tenantId").alias("SubscribedByUserTenantId")
        ,col("payload.After.subscriberIdentity.email").alias("SubscribedByUserEmail")
        ,col("payload.After.requestorIdentity.objectId").alias("RequestedByUserId")
        ,col("payload.After.requestorIdentity.tenantId").alias("RequestedByUserTenantId")
        ,col("payload.After.requestorIdentity.email").alias("RequestedByUserEmail")
        ,col("payload.After.writeAccess").alias("RequestWriteAccess")
        ,col("payload.After.accessDecisionDate").alias("RequestAccessDecisionDateTime")
        ,col("payload.After.version").alias("Version")
        ,col("payload.After.createdBy").alias("CreatedByUserId")
        ,col("payload.After.createdAt").alias("CreatedDatetime")
        ,col("payload.After.modifiedBy").alias("ModifiedByUserId")
        ,col("payload.After.modifiedAt").alias("ModifiedDateTime"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.Before.id").alias("SubscriberRequestId")
          ,col("payload.Before.dataProductId").alias("DataProductId")
          ,col("payload.Before.domainId").alias("BusinessDomainId")
          ,col("payload.Before.appliedPolicySet.id").alias("AccessPolicySetId")
          ,col("payload.Before.subscriberIdentity.identityType").alias("SubscriberIdentityTypeDisplayName")
          ,col("payload.Before.requestorIdentity.identityType").alias("RequestorIdentityTypeDisplayName")
          ,col("payload.Before.provisioningState").alias("SubscriberRequestStatus")
          ,col("payload.Before.subscriptionStatus").alias("RequestStatusDisplayName")
          ,col("payload.Before.subscriberIdentity.objectId").alias("SubscribedByUserId")
          ,col("payload.Before.subscriberIdentity.tenantId").alias("SubscribedByUserTenantId")
          ,col("payload.Before.subscriberIdentity.email").alias("SubscribedByUserEmail")
          ,col("payload.Before.requestorIdentity.objectId").alias("RequestedByUserId")
          ,col("payload.Before.requestorIdentity.tenantId").alias("RequestedByUserTenantId")
          ,col("payload.Before.requestorIdentity.email").alias("RequestedByUserEmail")
          ,col("payload.Before.writeAccess").alias("RequestWriteAccess")
          ,col("payload.Before.accessDecisionDate").alias("RequestAccessDecisionDateTime")
          ,col("payload.Before.version").alias("Version")
          ,col("payload.Before.createdBy").alias("CreatedByUserId")
          ,col("payload.Before.createdAt").alias("CreatedDatetime")
          ,col("payload.Before.modifiedBy").alias("ModifiedByUserId")
          ,col("payload.Before.modifiedAt").alias("ModifiedDateTime"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.select(
        col("SubscriberRequestId").cast(StringType).alias("SubscriberRequestId"),
        col("DataProductId").cast(StringType).alias("DataProductId"),
        col("BusinessDomainId").cast(StringType).alias("BusinessDomainId"),
        col("AccessPolicySetId").cast(StringType).alias("AccessPolicySetId"),
        col("SubscriberIdentityTypeDisplayName").cast(StringType).alias("SubscriberIdentityTypeDisplayName"),
        col("RequestorIdentityTypeDisplayName").cast(StringType).alias("RequestorIdentityTypeDisplayName"),
        col("SubscriberRequestStatus").cast(StringType).alias("SubscriberRequestStatus"),
        col("RequestStatusDisplayName").cast(StringType).alias("RequestStatusDisplayName"),
        col("SubscribedByUserId").cast(StringType).alias("SubscribedByUserId"),
        col("SubscribedByUserTenantId").cast(StringType).alias("SubscribedByUserTenantId"),
        col("SubscribedByUserEmail").cast(StringType).alias("SubscribedByUserEmail"),
        col("RequestedByUserId").cast(StringType).alias("RequestedByUserId"),
        col("RequestedByUserTenantId").cast(StringType).alias("RequestedByUserTenantId"),
        col("RequestedByUserEmail").cast(StringType).alias("RequestedByUserEmail"),
        col("RequestWriteAccess").cast(BooleanType).alias("RequestWriteAccess"),
        col("RequestAccessDecisionDateTime").cast(TimestampType).alias("RequestAccessDecisionDateTime"),
        col("Version").cast(StringType).alias("Version"),
        col("AccountId").cast(StringType).alias("AccountId"),
        col("CreatedDatetime").cast(TimestampType).alias("CreatedDatetime"),
        col("CreatedByUserId").cast(StringType).alias("CreatedByUserId"),
        col("ModifiedDateTime").cast(TimestampType).alias("ModifiedDateTime"),
        col("ModifiedByUserId").cast(StringType).alias("ModifiedByUserId"),
        col("EventProcessingTime").cast(LongType).alias("EventProcessingTime"),
        col("OperationType").cast(StringType).alias("OperationType")
      )

      val windowSpec = Window.partitionBy("SubscriberRequestId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess.filter(s"""SubscriberRequestId IS NOT NULL
                                      | AND DataProductId IS NOT NULL
                                      | AND BusinessDomainId IS NOT NULL
                                      | AND AccessPolicySetId IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      val filterString = s"""SubscriberRequestId is null
                            | or DataProductId is null
                            | or BusinessDomainId is null
                            | or AccessPolicySetId is null""".stripMargin
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataSubscriberRequest Data: ${e.getMessage}")
        logger.error(s"Error Processing DataSubscriberRequest Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataSubscriberRequest"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        var dfWrite = df
          .distinct()

        if (refreshType=="full"){
          dfWrite = dfWrite.filter(lower(col("OperationType")) =!= "delete")
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
                .filter(lower(col("OperationType")) =!= "delete")
                .filter(col("EventProcessingTime") > maxEventProcessingTime)
              dfTargetDeltaTable.as("target")
                .merge(
                  mergeDfSource.as("source"),
                  """target.SubscriberRequestId = source.SubscriberRequestId""")
                .whenMatched("source.ModifiedDatetime>=target.ModifiedDatetime")
                .updateAll()
                .whenNotMatched()
                .insertAll()
                .execute()
              val deleteDfSource = dfWrite
                .filter(col("EventProcessingTime") > maxEventProcessingTime)
                .filter(lower(col("OperationType")) === "delete")
              dfTargetDeltaTable.as("target")
                .merge(
                  deleteDfSource.as("source"),
                  expr("target.SubscriberRequestId = source.SubscriberRequestId")
                )
                .whenMatched
                .delete()
                .execute()
            }
            else if(!df.isEmpty && dfTarget.isEmpty){
              println("Delta Table DataSubscriberRequest Is Empty At Lake For incremental merge. Performing Full Overwrite...")
              dfWrite = dfWrite.filter(lower(col("OperationType")) =!= "delete")
              dfWrite.write
                .format("delta")
                .mode("overwrite")
                .save(adlsTargetDirectory.concat("/").concat(EntityName))
            }
          }
          else{
            println("Delta Table DataSubscriberRequest Does not exist for incremental merge. Performing Full Overwrite...")
            dfWrite = dfWrite.filter(lower(col("OperationType")) =!= "delete")
            dfWrite.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
      }
    }
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataSubscriberRequest data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataSubscriberRequest data: ${e.getMessage}")
        throw e
    }
  }
}
