package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class AccessPolicySetSchema {
  val accessPolicySetSchema: StructType = StructType(
    Array(
      StructField("AccessPolicySetId", StringType, nullable = false),
      StructField("ResourceTypeId", StringType, nullable = true),
      StructField("ProvisioningStateId", StringType, nullable = false),
      StructField("ActiveFlag", IntegerType, nullable = true),
      StructField("UseCaseExternalSharingPermittedFlag", IntegerType, nullable = true),
      StructField("UseCaseInternalSharingPermittedFlag", IntegerType, nullable = true),
      StructField("AttestationAcknowledgeTermsOfUseRequiredFlag", IntegerType, nullable = true),
      StructField("AttestationDataCopyPermittedFlag", IntegerType, nullable = true),
      StructField("PrivacyApprovalRequiredFlag", IntegerType, nullable = true),
      StructField("ManagerApprovalRequiredFlag", IntegerType, nullable = true),
      StructField("WriteAccessPermittedFlag", IntegerType, nullable = true),
      StructField("PolicyAppliedOnId", StringType, nullable = true),
      StructField("PolicyAppliedOn", StringType, nullable = true),
      StructField("AccessPolicySetVersion", IntegerType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false),
      StructField("ProvisioningStateDisplayName", StringType, nullable = false),
      StructField("AccessUseCaseDisplayName", StringType, nullable = true),
      StructField("AccessUseCaseDescription", StringType, nullable = true),
      StructField("ResourceType", StringType, nullable = true)
    )
  )
}