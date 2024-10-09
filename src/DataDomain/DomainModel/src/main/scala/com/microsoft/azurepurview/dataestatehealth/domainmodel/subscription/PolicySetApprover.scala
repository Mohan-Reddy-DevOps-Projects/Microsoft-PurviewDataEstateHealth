package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, explode_outer, expr, lower, max, row_number, when}
import org.apache.spark.sql.types.{BooleanType, LongType, StringType, TimestampType}

class PolicySetApprover (spark: SparkSession, logger:Logger){
  def processPolicySetApprover(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.After.id").alias("SubscriberRequestId")
        ,col("payload.After.appliedPolicySet.id").alias("AccessPolicySetId")
        ,col("payload.After.appliedPolicySet.policies.approvals").alias("appliedPolicySetPolicyApprovals")
        ,col("payload.After.appliedPolicySet.policies.termsOfUseRequired").alias("TermsOfUseRequired")
        ,col("payload.After.appliedPolicySet.policies.partnerExposurePermitted").alias("PartnerExposurePermitted")
        ,col("payload.After.appliedPolicySet.policies.customerExposurePermitted").alias("CustomerExposurePermitted")
        ,col("payload.After.appliedPolicySet.policies.privacyComplianceApprovalRequired").alias("PrivacyComplianceApprovalRequired")
        ,col("payload.After.appliedPolicySet.policies.managerApprovalRequired").alias("ManagerApprovalRequired")
        ,col("payload.After.appliedPolicySet.policies.dataCopyPermitted").alias("DataCopyPermitted")
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
          ,col("payload.Before.appliedPolicySet.id").alias("AccessPolicySetId")
          ,col("payload.Before.appliedPolicySet.policies.approvals").alias("appliedPolicySetPolicyApprovals")
          ,col("payload.Before.appliedPolicySet.policies.termsOfUseRequired").alias("TermsOfUseRequired")
          ,col("payload.Before.appliedPolicySet.policies.partnerExposurePermitted").alias("PartnerExposurePermitted")
          ,col("payload.Before.appliedPolicySet.policies.customerExposurePermitted").alias("CustomerExposurePermitted")
          ,col("payload.Before.appliedPolicySet.policies.privacyComplianceApprovalRequired").alias("PrivacyComplianceApprovalRequired")
          ,col("payload.Before.appliedPolicySet.policies.managerApprovalRequired").alias("ManagerApprovalRequired")
          ,col("payload.Before.appliedPolicySet.policies.dataCopyPermitted").alias("DataCopyPermitted")
          ,col("payload.Before.createdBy").alias("CreatedByUserId")
          ,col("payload.Before.createdAt").alias("CreatedDatetime")
          ,col("payload.Before.modifiedBy").alias("ModifiedByUserId")
          ,col("payload.Before.modifiedAt").alias("ModifiedDateTime"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.withColumn("afterExplode_appliedPolicySetPolicyApprovals", explode_outer(col("appliedPolicySetPolicyApprovals")))
      dfProcess = dfProcess.withColumn("ApproverIdentityType",col("afterExplode_appliedPolicySetPolicyApprovals.identityType"))
        .withColumn("ApproverUserId",col("afterExplode_appliedPolicySetPolicyApprovals.objectId"))
        .withColumn("ApproverUserTenantId",col("afterExplode_appliedPolicySetPolicyApprovals.tenantId"))
        .withColumn("ApproverUserEmail",col("afterExplode_appliedPolicySetPolicyApprovals.email"))
        .drop("afterExplode_appliedPolicySetPolicyApprovals")

      dfProcess = dfProcess.select(
        col("SubscriberRequestId").cast(StringType).alias("SubscriberRequestId"),
        col("AccessPolicySetId").cast(StringType).alias("AccessPolicySetId"),
        col("ApproverUserId").cast(StringType).alias("ApproverUserId"),
        col("ApproverIdentityType").cast(StringType).alias("ApproverIdentityType"),
        col("ApproverUserTenantId").cast(StringType).alias("ApproverUserTenantId"),
        col("ApproverUserEmail").cast(StringType).alias("ApproverUserEmail"),
        col("TermsOfUseRequired").cast(BooleanType).alias("TermsOfUseRequired"),
        col("PartnerExposurePermitted").cast(BooleanType).alias("PartnerExposurePermitted"),
        col("CustomerExposurePermitted").cast(BooleanType).alias("CustomerExposurePermitted"),
        col("PrivacyComplianceApprovalRequired").cast(BooleanType).alias("PrivacyComplianceApprovalRequired"),
        col("ManagerApprovalRequired").cast(BooleanType).alias("ManagerApprovalRequired"),
        col("DataCopyPermitted").cast(BooleanType).alias("DataCopyPermitted"),
        col("AccountId").cast(StringType).alias("AccountId"),
        col("CreatedDatetime").cast(TimestampType).alias("CreatedDatetime"),
        col("CreatedByUserId").cast(StringType).alias("CreatedByUserId"),
        col("ModifiedDateTime").cast(TimestampType).alias("ModifiedDateTime"),
        col("ModifiedByUserId").cast(StringType).alias("ModifiedByUserId"),
        col("EventProcessingTime").cast(LongType).alias("EventProcessingTime"),
        col("OperationType").cast(StringType).alias("OperationType")
      )

      val windowSpec = Window.partitionBy("SubscriberRequestId"
        ,"AccessPolicySetId"
        ,"ApproverIdentityType"
        ,"ApproverUserId"
        ,"ApproverUserTenantId").orderBy(col("ModifiedDateTime").desc,
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
                                      | AND AccessPolicySetId IS NOT NULL
                                      | AND ApproverUserId IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      val filterString = s"""SubscriberRequestId is null
                            | or AccessPolicySetId is null
                            | or ApproverUserId is null""".stripMargin
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing PolicySetApprover Data: ${e.getMessage}")
        logger.error(s"Error Processing PolicySetApprover Data: ${e.getMessage}")
        throw e
    }
  }
}
