package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDateSchema {
  val dimDateSchema: StructType = StructType(
    Array(
      StructField("DateId", LongType, nullable = false),
      StructField("YearId", IntegerType, nullable = false),
      StructField("YearName", StringType, nullable = false),
      StructField("CalendarSemesterId", IntegerType, nullable = false),
      StructField("CalendarSemesterDisplayName", StringType, nullable = false),
      StructField("YearSemester", StringType, nullable = false),
      StructField("CalendarQuarterId", IntegerType, nullable = false),
      StructField("CalendarQuarterDisplayName", StringType, nullable = false),
      StructField("YearQuarter", StringType, nullable = false),
      StructField("MonthId", IntegerType, nullable = false),
      StructField("MonthDisplayName", StringType, nullable = false),
      StructField("YearMonth", StringType, nullable = false),
      StructField("CalendarWeekId", IntegerType, nullable = false),
      StructField("YearWeek", StringType, nullable = false),
      StructField("CalendarDateId", IntegerType, nullable = false),
      StructField("CalendarDayDisplayName", StringType, nullable = false),
      StructField("CalendarDayDate", TimestampType, nullable = false)

    )
  )
}
