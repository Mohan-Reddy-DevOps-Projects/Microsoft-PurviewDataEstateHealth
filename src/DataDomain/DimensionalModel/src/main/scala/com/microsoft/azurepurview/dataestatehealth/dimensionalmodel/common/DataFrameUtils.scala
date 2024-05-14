package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._
class DataFrameUtils (spark: SparkSession, logger:Logger) {
  def getMaxId(df: DataFrame, columnName: String): Long = {
    try {
      val maxRow = df.select(max(coalesce(col(columnName), lit(0)))).head()
      maxRow.getLong(0)
    } catch {
      case e: Exception =>
        println(s"An error occurred while retrieving the maximum value: ${e.getMessage}")
        0L // Return a default value in case of an error
    }
  }
}