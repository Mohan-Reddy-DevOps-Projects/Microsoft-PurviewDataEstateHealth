package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetColumnClassificationAssignmentSchema {
  val dataAssetColumnClassificationAssignmentSchema: StructType = StructType(
    Array(
      StructField("DataAssetId", StringType, nullable = false),
      StructField("ColumnId", StringType, nullable = false),
      StructField("ClassificationId", StringType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true)
    )
  )
}