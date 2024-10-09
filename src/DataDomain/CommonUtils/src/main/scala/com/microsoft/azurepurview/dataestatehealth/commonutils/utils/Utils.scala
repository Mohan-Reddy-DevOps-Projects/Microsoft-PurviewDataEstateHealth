package com.microsoft.azurepurview.dataestatehealth.commonutils.utils

import org.apache.spark.sql.types._
import org.apache.spark.sql.{DataFrame, Row, SparkSession}

object Utils {
  def createColdStartDataFrame(spark: SparkSession, schema: StructType): DataFrame = {

    def createDummyValue(field: StructField): Any = {
      field.dataType match {
        case IntegerType => 0
        case LongType => 0L
        case FloatType => 0.0f
        case DoubleType => 0.0
        case StringType => "NA"
        case BooleanType => false
        case DateType => java.sql.Date.valueOf("2000-01-01")
        case TimestampType => java.sql.Timestamp.valueOf("2000-01-01 00:00:00")
        case DecimalType() => BigDecimal(0)
        case BinaryType => Array.empty[Byte]
        case ArrayType(_, _) => Array.empty // Use an empty array for ArrayType
        case _ => null // Handle other types or leave as null
      }
    }

    // Generate a single dummy record based on the schema
    val coldStartRow = Row.fromSeq(schema.fields.map(createDummyValue))

    // Create DataFrame with the dummy row
    val data = Seq(coldStartRow) // Use Seq for consistency
    spark.createDataFrame(spark.sparkContext.parallelize(data), schema)
  }
}
