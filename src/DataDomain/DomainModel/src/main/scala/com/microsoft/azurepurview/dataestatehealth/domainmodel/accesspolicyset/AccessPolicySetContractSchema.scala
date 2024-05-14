package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.spark.sql.types._
class AccessPolicySetContractSchema {
  val policySetContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("Before", StructType(Seq(
        StructField("policies", StructType(Seq(
          StructField("approvals", ArrayType(StructType(Seq(
            StructField("identityType", StringType),
            StructField("objectId", StringType),
            StructField("tenantId", StringType),
            StructField("displayName", StringType)
          )))),
          StructField("maximumAccessDuration", StructType(Seq(
            StructField("durationType", StringType),
            StructField("length", IntegerType)
          ))),
          StructField("permittedUseCases", ArrayType(StructType(Seq(
            StructField("Title", StringType),
            StructField("Description", StringType)
          )))),
          StructField("termsOfUseRequired", BooleanType),
          StructField("partnerExposurePermitted", BooleanType),
          StructField("customerExposurePermitted", BooleanType),
          StructField("privacyComplianceApprovalRequired", BooleanType),
          StructField("attestations", ArrayType(StructType(Seq(
            StructField("required", BooleanType),
            StructField("displayName", StringType),
            StructField("documentReference", StringType)
          )))),
          StructField("managerApprovalRequired", BooleanType),
          StructField("dataCopyPermitted", BooleanType)
        ))),
        StructField("id", StringType),
        StructField("accountId", StringType),
        StructField("targetResource", StructType(Seq(
          StructField("TargetId", StringType),
          StructField("TargetType", StringType)
        ))),
        StructField("active", BooleanType),
        StructField("version", StringType),
        StructField("createdAt", StringType),
        StructField("createdBy", StringType),
        StructField("modifiedAt", StringType),
        StructField("modifiedBy", StringType),
        StructField("provisioningState", StringType),
        StructField("policySetKind", StringType)
      ))),
      StructField("After", StructType(Seq(
        StructField("policies", StructType(Seq(
          StructField("approvals", ArrayType(StructType(Seq(
            StructField("identityType", StringType),
            StructField("objectId", StringType),
            StructField("tenantId", StringType),
            StructField("displayName", StringType)
          )))),
          StructField("maximumAccessDuration", StructType(Seq(
            StructField("durationType", StringType),
            StructField("length", IntegerType)
          ))),
          StructField("permittedUseCases", ArrayType(StructType(Seq(
            StructField("Title", StringType),
            StructField("Description", StringType)
          )))),
          StructField("termsOfUseRequired", BooleanType),
          StructField("partnerExposurePermitted", BooleanType),
          StructField("customerExposurePermitted", BooleanType),
          StructField("privacyComplianceApprovalRequired", BooleanType),
          StructField("attestations", ArrayType(StructType(Seq(
            StructField("required", BooleanType),
            StructField("displayName", StringType),
            StructField("documentReference", StringType)
          )))),
          StructField("managerApprovalRequired", BooleanType),
          StructField("dataCopyPermitted", BooleanType)
        ))),
        StructField("id", StringType),
        StructField("accountId", StringType),
        StructField("targetResource", StructType(Seq(
          StructField("TargetId", StringType),
          StructField("TargetType", StringType)
        ))),
        StructField("active", BooleanType),
        StructField("version", StringType),
        StructField("createdAt", StringType),
        StructField("createdBy", StringType),
        StructField("modifiedAt", StringType),
        StructField("modifiedBy", StringType),
        StructField("provisioningState", StringType),
        StructField("policySetKind", StringType)
      )))
    ))),
    StructField("payloadKind", StringType),
    StructField("operationType", StringType),
    StructField("eventId", StringType),
    StructField("correlationId", StringType),
    StructField("eventSource", StringType),
    StructField("preciseTimestamp", StringType),
    StructField("tenantId", StringType),
    StructField("accountId", StringType),
    StructField("triggeredBy", StringType),
    StructField("EventProcessedUtcTime", StringType),
    StructField("PartitionId", IntegerType),
    StructField("EventEnqueuedUtcTime", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
}
