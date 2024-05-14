package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription
import org.apache.spark.sql.types._
class DataSubscriberRequestSchema {
  val dataSubscriberRequestSchema: StructType = StructType(
    Array(
      StructField("SubscriberRequestId", StringType, nullable = false),
      StructField("DataProductId", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("AccessPolicySetId", StringType, nullable = false),
      StructField("SubscriberIdentityTypeDisplayName", StringType, nullable = true),
      StructField("RequestorIdentityTypeDisplayName", StringType, nullable = true),
      StructField("SubscriberRequestStatus", StringType, nullable = true),
      StructField("RequestStatusDisplayName", StringType, nullable = true),
      StructField("SubscribedByUserId", StringType, nullable = true),
      StructField("SubscribedByUserTenantId", StringType, nullable = true),
      StructField("SubscribedByUserEmail", StringType, nullable = true),
      StructField("RequestedByUserId", StringType, nullable = true),
      StructField("RequestedByUserTenantId", StringType, nullable = true),
      StructField("RequestedByUserEmail", StringType, nullable = true),
      StructField("RequestWriteAccess", BooleanType, nullable = true),
      StructField("RequestAccessDecisionDateTime", TimestampType, nullable = true),
      StructField("Version", StringType, nullable = true),
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