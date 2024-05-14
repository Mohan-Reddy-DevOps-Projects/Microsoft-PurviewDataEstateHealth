package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}

import java.sql.{Date, Timestamp}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types._

class DimDate (spark: SparkSession, logger:Logger){
  def populateDimTimeTable(schema: org.apache.spark.sql.types.StructType):DataFrame= {

    try {
      val startDate = Date.valueOf("2000-01-01")
      val endDate = Date.valueOf(java.time.LocalDate.now().plusYears(10))

      val dateSeq = spark.range(0, (endDate.getTime - startDate.getTime) / (24 * 60 * 60 * 1000), 1)
        .selectExpr(s"date_add(DATE '$startDate', cast(id as int)) as date")

      var dimDate = dateSeq.select(concat(year(col("date")),
        format_string("%02d", month(col("date"))),
        format_string("%02d", dayofmonth(col("date")))).cast("bigint").alias("DateId"),
        year(col("date")).alias("YearId"),
        year(col("date")).cast("string").alias("YearName"),
        ceil(month(col("date")) / 6).alias("CalendarSemesterId"),
        when(ceil(month(col("date")) / 6) === 1, "First Semester").otherwise("Second Semester").alias("CalendarSemesterDisplayName"),
        concat(year(col("date")), lit(" "), when(ceil(month(col("date")) / 6) === 1, "First Semester").otherwise("Second Semester")).alias("YearSemester"),
        concat(year(col("date")), lit(" "),
          when(ceil(month(col("date")) / 3) === 1, "First Quarter")
            .when(ceil(month(col("date")) / 3) === 2, "Second Quarter")
            .when(ceil(month(col("date")) / 3) === 3, "Third Quarter")
            .otherwise("Fourth Quarter")).alias("YearQuarter"),
        quarter(col("date")).alias("CalendarQuarterId"),
        when(quarter(col("date")) === 1, "First Quarter")
          .when(quarter(col("date")) === 2, "Second Quarter")
          .when(quarter(col("date")) === 3, "Third Quarter")
          .otherwise("Fourth Quarter").alias("CalendarQuarterDisplayName"),
        month(col("date")).alias("MonthId"),
        date_format(col("date"), "MMMM").alias("MonthDisplayName"),
        concat(year(col("date")), lit(" "), date_format(col("date"), "MMMM")).alias("YearMonth"),
        weekofyear(col("date")).alias("CalendarWeekId"),
        concat(year(col("date")), lit(" Week "), weekofyear(col("date"))).alias("YearWeek"),
        dayofmonth(col("date")).alias("CalendarDateId"),
        date_format(col("date"), "EEEE").alias("CalendarDayDisplayName"),
        col("date").cast(TimestampType).alias("CalendarDayDate")
      ).orderBy("DateId")

      dimDate = dimDate.select(col("DateId").cast(LongType)
        ,col("YearId").cast(IntegerType)
        ,col("YearName").cast(StringType)
        ,col("CalendarSemesterId").cast(IntegerType)
        ,col("CalendarSemesterDisplayName").cast(StringType)
        ,col("YearSemester").cast(StringType)
        ,col("CalendarQuarterId").cast(IntegerType)
        ,col("CalendarQuarterDisplayName").cast(StringType)
        ,col("YearQuarter").cast(StringType)
        ,col("MonthId").cast(IntegerType)
        ,col("MonthDisplayName").cast(StringType)
        ,col("YearMonth").cast(StringType)
        ,col("CalendarWeekId").cast(IntegerType)
        ,col("YearWeek").cast(StringType)
        ,col("CalendarDateId").cast(IntegerType)
        ,col("CalendarDayDisplayName").cast(StringType)
        ,col("CalendarDayDate").cast(TimestampType)
      )
      val dfProcessed = spark.createDataFrame(dimDate.rdd, schema=schema)
      val filterString = s"""DateId is null
                            | or CalendarDayDate is null
                            | or YearId is null
                            | or MonthId is null
                            | or CalendarDateId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    } catch {
      case e: Exception =>
        println(s"Error Processing DimDate Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDate Dimension: ${e.getMessage}")
        throw e
    }
  }
}
