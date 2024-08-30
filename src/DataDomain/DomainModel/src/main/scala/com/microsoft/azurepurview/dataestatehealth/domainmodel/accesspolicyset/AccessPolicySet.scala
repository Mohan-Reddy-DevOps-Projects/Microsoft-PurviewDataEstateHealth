package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, explode_outer, expr, lit, lower, max, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class AccessPolicySet (spark: SparkSession, logger:Logger){
  def processAccessPolicySet(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("AccessPolicySetId")
        ,col("payload.after.targetResource.TargetId").alias("ResourceTypeId")
        ,col("payload.after.active").alias("ActiveFlag")
        ,col("payload.after.policies.partnerExposurePermitted").alias("UseCaseExternalSharingPermittedFlag")
        ,col("payload.after.policies.customerExposurePermitted").alias("UseCaseInternalSharingPermittedFlag")
        ,col("payload.after.policies.termsOfUseRequired").alias("AttestationAcknowledgeTermsOfUseRequiredFlag")
        ,col("payload.after.policies.dataCopyPermitted").alias("AttestationDataCopyPermittedFlag")
        ,col("payload.after.policies.privacyComplianceApprovalRequired").alias("PrivacyApprovalRequiredFlag")
        ,col("payload.after.policies.managerApprovalRequired").alias("ManagerApprovalRequiredFlag")
        ,col("payload.after.policies.dataCopyPermitted").alias("WriteAccessPermittedFlag")
        ,col("payload.after.targetResource.TargetId").alias("PolicyAppliedOnId")
        ,col("payload.after.targetResource.TargetType").alias("PolicyAppliedOn")
        ,col("payload.after.version").alias("AccessPolicySetVersion")
        ,col("payload.after.createdBy").alias("CreatedByUserId")
        ,col("payload.after.createdAt").alias("CreatedDatetime")
        ,col("payload.after.modifiedBy").alias("ModifiedByUserId")
        ,col("payload.after.modifiedAt").alias("ModifiedDateTime")
        ,col("payload.after.targetResource.TargetType").alias("ResourceType")
        ,col("payload.after.provisioningState").alias("ProvisioningStateDisplayName")
        ,col("payload.after.policies.permittedUseCases").alias("PermittedUseCases"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.id").alias("AccessPolicySetId")
          ,col("payload.before.targetResource.TargetId").alias("ResourceTypeId")
          ,col("payload.before.active").alias("ActiveFlag")
          ,col("payload.before.policies.partnerExposurePermitted").alias("UseCaseExternalSharingPermittedFlag")
          ,col("payload.before.policies.customerExposurePermitted").alias("UseCaseInternalSharingPermittedFlag")
          ,col("payload.before.policies.termsOfUseRequired").alias("AttestationAcknowledgeTermsOfUseRequiredFlag")
          ,col("payload.before.policies.dataCopyPermitted").alias("AttestationDataCopyPermittedFlag")
          ,col("payload.before.policies.privacyComplianceApprovalRequired").alias("PrivacyApprovalRequiredFlag")
          ,col("payload.before.policies.managerApprovalRequired").alias("ManagerApprovalRequiredFlag")
          ,col("payload.before.policies.dataCopyPermitted").alias("WriteAccessPermittedFlag")
          ,col("payload.before.targetResource.TargetId").alias("PolicyAppliedOnId")
          ,col("payload.before.targetResource.TargetType").alias("PolicyAppliedOn")
          ,col("payload.before.version").alias("AccessPolicySetVersion")
          ,col("payload.before.createdBy").alias("CreatedByUserId")
          ,col("payload.before.createdAt").alias("CreatedDatetime")
          ,col("payload.before.modifiedBy").alias("ModifiedByUserId")
          ,col("payload.before.modifiedAt").alias("ModifiedDateTime")
          ,col("payload.before.targetResource.TargetType").alias("ResourceType")
          ,col("payload.before.provisioningState").alias("ProvisioningStateDisplayName")
          ,col("payload.before.policies.permittedUseCases").alias("PermittedUseCases"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }
      dfProcess = dfProcess.withColumn("PermittedUseCases_exploded",explode_outer(col("PermittedUseCases")))
      dfProcess = dfProcess.withColumn("AccessUseCaseDisplayName",col("PermittedUseCases_exploded.Title"))
        .withColumn("AccessUseCaseDescription",col("PermittedUseCases_exploded.Description"))
        .drop("PermittedUseCases_exploded")
        .drop("PermittedUseCases")

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("ProvisioningStateDisplayName"),"ProvisioningStateId")

      dfProcess = dfProcess.select(col("AccessPolicySetId").cast(StringType)
        ,col("ResourceTypeId").cast(StringType)
        ,col("ProvisioningStateId").cast(StringType)
        ,col("ActiveFlag").cast(IntegerType)
        ,col("UseCaseExternalSharingPermittedFlag").cast(IntegerType)
        ,col("UseCaseInternalSharingPermittedFlag").cast(IntegerType)
        ,col("AttestationAcknowledgeTermsOfUseRequiredFlag").cast(IntegerType)
        ,col("AttestationDataCopyPermittedFlag").cast(IntegerType)
        ,col("PrivacyApprovalRequiredFlag").cast(IntegerType)
        ,col("ManagerApprovalRequiredFlag").cast(IntegerType)
        ,col("WriteAccessPermittedFlag").cast(IntegerType)
        ,col("PolicyAppliedOnId").cast(StringType)
        ,col("PolicyAppliedOn").cast(StringType)
        ,col("AccessPolicySetVersion").cast(IntegerType)
        ,col("AccountId").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("CreatedByUserId").cast(StringType)
        ,col("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
        ,col("operationType").alias("OperationType").cast(StringType)
        ,col("ProvisioningStateDisplayName").cast(StringType)
        ,col("AccessUseCaseDisplayName").cast(StringType)
        ,col("AccessUseCaseDescription").cast(StringType)
        ,col("ResourceType").cast(StringType)
      )

      val windowSpec = Window.partitionBy("AccessPolicySetId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess.filter(s"""AccessPolicySetId IS NOT NULL
                                      | AND ProvisioningStateDisplayName IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      val filterString = s"""AccessPolicySetId is null
                            | or ResourceTypeId is null""".stripMargin
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing AccessPolicySet Data: ${e.getMessage}")
        logger.error(s"Error Processing AccessPolicySet Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "AccessPolicySet"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        var dfWrite = df
          .drop("ProvisioningStateDisplayName")
          .drop("PermittedUseCasesTitle")
          .drop("PermittedUseCasesDescription")
          .drop("ResourceType")
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
                  """target.AccessPolicySetId = source.AccessPolicySetId""")
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
                  expr("target.AccessPolicySetId = source.AccessPolicySetId")
                )
                .whenMatched
                .delete()
                .execute()
            } else if(!df.isEmpty && dfTarget.isEmpty){
              println("Delta Table AccessPolicySet Is Empty At Lake For incremental merge. Performing Full Overwrite...")
              dfWrite = dfWrite.filter(lower(col("OperationType")) =!= "delete")
              dfWrite.write
                .format("delta")
                .mode("overwrite")
                .save(adlsTargetDirectory.concat("/").concat(EntityName))
            }
          }
          else{
            println("Delta Table AccessPolicySet Does not exist for incremental merge. Performing Full Overwrite...")
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
        println(s"Error Writing/Merging AccessPolicySet data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging AccessPolicySet data: ${e.getMessage}")
        throw e
    }
  }
}
