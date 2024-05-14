package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class ClassificationSchema {
  val classificationSchema: StructType = StructType(
    Array(
      StructField("ClassificationId", StringType, nullable = false),
      StructField("ClassificationDisplayName", StringType, nullable = false),
        StructField("ClassificationDescription", StringType, nullable = false)
    )
  )
}
