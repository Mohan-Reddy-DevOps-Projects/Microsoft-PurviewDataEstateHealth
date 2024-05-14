package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class DimDataHealthControlSchema {
  val dimDataHealthControlSchema: StructType = StructType(
    Array(
      StructField("HealthControlId", IntegerType, nullable = false),
      StructField("HealthControlDisplayName", StringType, nullable = false),
      StructField("HealthControlGroupDisplayName", StringType, nullable = false),
      StructField("HealthControlDefinition", StringType, nullable = false),
      StructField("HealthControlScope", StringType, nullable = false),
      StructField("CDMCControlMapping", StringType, nullable = false)
    )
  )
}
