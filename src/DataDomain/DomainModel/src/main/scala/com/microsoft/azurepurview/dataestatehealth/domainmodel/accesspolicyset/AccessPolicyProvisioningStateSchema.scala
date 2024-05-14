package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class AccessPolicyProvisioningStateSchema {
  val accessPolicyProvisioningStateSchema: StructType = StructType(
    Array(
      StructField("ProvisioningStateId", StringType, nullable = false),
      StructField("ProvisioningStateDisplayName", StringType, nullable = true)
    )
  )
}
