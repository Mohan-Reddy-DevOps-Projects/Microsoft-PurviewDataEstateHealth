package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription
import org.apache.spark.sql.types._
class SubscriptionContractSchema {
  val subscriptionContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("Before", StructType(Seq(
        StructField("id", StringType),
        StructField("AccountId", StringType),
        StructField("writeAccess", BooleanType),
        StructField("subscriptionStatus", StringType),
        StructField("subscriberIdentity", StructType(Seq(
          StructField("identityType", StringType),
          StructField("objectId", StringType),
          StructField("tenantId", StringType),
          StructField("displayName", StringType),
          StructField("email", StringType)
        ))),
        StructField("requestorIdentity", StructType(Seq(
          StructField("identityType", StringType),
          StructField("objectId", StringType),
          StructField("tenantId", StringType),
          StructField("displayName", StringType),
          StructField("email", StringType)
        ))),
        StructField("domainId", StringType),
        StructField("dataProductId", StringType),
        StructField("policySetValues", StructType(Seq(
          StructField("accessDuration", StructType(Seq(
            StructField("durationType", StringType),
            StructField("length", IntegerType)
          ))),
          StructField("useCase", StringType),
          StructField("businessJustification", StringType),
          StructField("approverDecisions", ArrayType(StructType(Seq(
            StructField("approver", StructType(Seq(
              StructField("identityType", StringType),
              StructField("objectId", StringType),
              StructField("tenantId", StringType),
              StructField("displayName", StringType),
              StructField("email", StringType)
            ))),
            StructField("approverDecisionType", StringType),
            StructField("decision", StringType),
            StructField("comments", StringType)
          )))),
          StructField("partnerSharingRequested", BooleanType),
          StructField("customerSharingRequested", BooleanType),
          StructField("termsOfUseAccepted", BooleanType)
        ))),
        StructField("provisioningState", StringType),
        StructField("createdAt", StringType),
        StructField("createdBy", StringType),
        StructField("modifiedAt", StringType),
        StructField("modifiedBy", StringType),
        StructField("version", StringType),
        StructField("appliedPolicySet", StructType(Seq(
          StructField("policies", StructType(Seq(
            StructField("approvals", ArrayType(StructType(Seq(
              StructField("identityType", StringType),
              StructField("objectId", StringType),
              StructField("tenantId", StringType),
              StructField("displayName", StringType),
              StructField("email", StringType)
            )))),
            StructField("permittedUseCases", ArrayType(StructType(Seq(
              StructField("Title", StringType),
              StructField("Description", StringType)
            )))),
            StructField("termsOfUseRequired", BooleanType),
            StructField("partnerExposurePermitted", BooleanType),
            StructField("customerExposurePermitted", BooleanType),
            StructField("privacyComplianceApprovalRequired", BooleanType),
            StructField("attestations", ArrayType(StringType)),
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
        StructField("accessDecisionDate", StringType),
        StructField("allowedReaders", ArrayType(StringType))
      ))),
      StructField("After", StructType(Seq(
        StructField("id", StringType),
        StructField("AccountId", StringType),
        StructField("writeAccess", BooleanType),
        StructField("subscriptionStatus", StringType),
        StructField("subscriberIdentity", StructType(Seq(
          StructField("identityType", StringType),
          StructField("objectId", StringType),
          StructField("tenantId", StringType),
          StructField("displayName", StringType),
          StructField("email", StringType)
        ))),
        StructField("requestorIdentity", StructType(Seq(
          StructField("identityType", StringType),
          StructField("objectId", StringType),
          StructField("tenantId", StringType),
          StructField("displayName", StringType),
          StructField("email", StringType)
        ))),
        StructField("domainId", StringType),
        StructField("domainName", StringType),
        StructField("dataProductId", StringType),
        StructField("dataProductName", StringType),
        StructField("dataProductType", StringType),
        StructField("policySetValues", StructType(Seq(
          StructField("accessDuration", StructType(Seq(
            StructField("durationType", StringType),
            StructField("length", IntegerType)
          ))),
          StructField("useCase", StringType),
          StructField("businessJustification", StringType),
          StructField("approverDecisions", ArrayType(StructType(Seq(
            StructField("approver", StructType(Seq(
              StructField("identityType", StringType),
              StructField("objectId", StringType),
              StructField("tenantId", StringType),
              StructField("displayName", StringType),
              StructField("email", StringType)
            ))),
            StructField("approverDecisionType", StringType),
            StructField("decision", StringType),
            StructField("comments", StringType)
          )))),
          StructField("partnerSharingRequested", BooleanType),
          StructField("customerSharingRequested", BooleanType),
          StructField("termsOfUseAccepted", BooleanType)
        ))),
        StructField("provisioningState", StringType),
        StructField("createdAt", StringType),
        StructField("createdBy", StringType),
        StructField("modifiedAt", StringType),
        StructField("modifiedBy", StringType),
        StructField("version", StringType),
        StructField("appliedPolicySet", StructType(Seq(
          StructField("policies", StructType(Seq(
            StructField("approvals", ArrayType(StructType(Seq(
              StructField("identityType", StringType),
              StructField("objectId", StringType),
              StructField("tenantId", StringType),
              StructField("displayName", StringType),
              StructField("email", StringType)
            )))),
            StructField("permittedUseCases", ArrayType(StructType(Seq(
              StructField("Title", StringType),
              StructField("Description", StringType)
            )))),
            StructField("termsOfUseRequired", BooleanType),
            StructField("partnerExposurePermitted", BooleanType),
            StructField("customerExposurePermitted", BooleanType),
            StructField("privacyComplianceApprovalRequired", BooleanType),
            StructField("attestations", ArrayType(StringType)),
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
      StructField("accessDecisionDate", StringType),
      StructField("allowedReaders", ArrayType(StringType))
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
