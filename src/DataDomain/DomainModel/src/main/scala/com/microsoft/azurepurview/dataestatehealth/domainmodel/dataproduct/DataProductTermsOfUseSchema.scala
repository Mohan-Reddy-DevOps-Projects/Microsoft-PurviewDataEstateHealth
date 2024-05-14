package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataProductTermsOfUseSchema {
  val dataProductTermsOfUseSchema: StructType = StructType(
    Array(
      StructField("DataProductId", StringType, nullable = false),
      StructField("TermsOfUseId", StringType, nullable = false),
      StructField("TermsOfUseDisplayName", StringType, nullable = false),
      StructField("TermsOfUseHyperlink", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = true)
    )
  )
}