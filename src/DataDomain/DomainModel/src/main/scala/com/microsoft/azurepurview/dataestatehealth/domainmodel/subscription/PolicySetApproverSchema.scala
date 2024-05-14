package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription
import org.apache.spark.sql.types._
class PolicySetApproverSchema {
  val policySetApproverSchema: StructType = StructType(
    Array(
      StructField("SubscriberRequestId", StringType, nullable = false),
      StructField("AccessPolicySetId", StringType, nullable = false),
      StructField("ApproverUserId", StringType, nullable = false),
      StructField("ApproverIdentityType", StringType, nullable = true),
      StructField("ApproverUserTenantId", StringType, nullable = true),
      StructField("ApproverUserEmail", StringType, nullable = true),
      StructField("TermsOfUseRequired", BooleanType, nullable = true),
      StructField("PartnerExposurePermitted", BooleanType, nullable = true),
      StructField("CustomerExposurePermitted", BooleanType, nullable = true),
      StructField("PrivacyComplianceApprovalRequired", BooleanType, nullable = true),
      StructField("ManagerApprovalRequired", BooleanType, nullable = true),
      StructField("DataCopyPermitted", BooleanType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
