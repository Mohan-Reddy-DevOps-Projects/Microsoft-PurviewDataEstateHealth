package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataProductAssetAssignmentSchema {
  val dataProductAssetAssignmentSchema: StructType = StructType(
    Array(
      StructField("DataProductId", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = false),
      StructField("AssignedByUserId", StringType, nullable = true),
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