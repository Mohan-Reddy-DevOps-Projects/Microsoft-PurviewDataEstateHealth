package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetOwnerAssignmentSchema {
  val dataAssetOwnerAssignmentSchema: StructType = StructType(
    Array(
      StructField("DataAssetId", StringType, nullable = false),
      StructField("DataAssetOwnerId", StringType, nullable = false),
      StructField("AssignedByUserId", StringType, nullable = false),
      StructField("ActiveFlagLastModifiedDatetime", TimestampType, nullable = true),
      StructField("AssignmentLastModifiedDatetime", TimestampType, nullable = true),
      StructField("ActiveFlag", IntegerType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
