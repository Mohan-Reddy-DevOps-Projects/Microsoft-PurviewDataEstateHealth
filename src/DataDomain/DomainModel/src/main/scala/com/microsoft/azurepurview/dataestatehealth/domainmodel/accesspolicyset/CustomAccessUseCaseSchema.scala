package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class CustomAccessUseCaseSchema {
  val customAccessUseCaseSchema: StructType = StructType(
    Array(
      StructField("AccessPolicySetId", StringType, nullable = false),
      StructField("AccessUseCaseDisplayName", StringType, nullable = true),
      StructField("AccessUseCaseDescription", StringType, nullable = true)
    )
  )
}
