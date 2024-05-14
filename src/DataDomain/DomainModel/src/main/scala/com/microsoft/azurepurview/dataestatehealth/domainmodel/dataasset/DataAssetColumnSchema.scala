package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetColumnSchema {
  val dataAssetColumnSchema: StructType = StructType(
    Array(
      StructField("DataAssetId", StringType, nullable = false),
      StructField("ColumnId", StringType, nullable = false),
      StructField("ColumnDisplayName", StringType, nullable = false),
      StructField("ColumnDescription", StringType, nullable = true),
      StructField("DataAssetTypeId", StringType, nullable = false),
      StructField("DataTypeId", StringType, nullable = false),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false),
      StructField("ClassificationId", StringType, nullable = true),
      StructField("ColumnDataType", StringType, nullable = false),
      StructField("ColumnClassification", StringType, nullable = true)
    )
  )
}