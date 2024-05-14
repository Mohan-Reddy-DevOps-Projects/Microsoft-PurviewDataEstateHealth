package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class AccessPolicyResourceTypeSchema {
  val accessPolicyResourceTypeSchema: StructType = StructType(
    Array(
      StructField("ResourceTypeId", StringType, nullable = false),
      StructField("ResourceTypeDisplayName", StringType, nullable = true)
    )
  )
}
