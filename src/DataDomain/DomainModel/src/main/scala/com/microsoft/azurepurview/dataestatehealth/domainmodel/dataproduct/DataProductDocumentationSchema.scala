package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataProductDocumentationSchema {
  val dataProductDocumentationSchema: StructType = StructType(
    Array(
      StructField("DataProductId", StringType, nullable = false),
      StructField("DocumentationId", StringType, nullable = false),
      StructField("DocumentationDisplayName", StringType, nullable = false),
      StructField("DocumentationHyperlink", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = true)
    )
  )
}
