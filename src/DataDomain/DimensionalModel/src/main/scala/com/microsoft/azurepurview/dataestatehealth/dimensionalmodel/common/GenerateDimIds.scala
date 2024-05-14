package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types.TimestampType

class GenerateDimIds(spark: SparkSession) {
  def appendIds(newData: DataFrame, targetData: DataFrame, uuidColumn: String, datetimeColumn: String, idColumnName: String): DataFrame = {
    val maxId = if (targetData.isEmpty) 0L
    else targetData.agg(max(idColumnName)).collect()(0).getLong(0)

    val withIds = if (maxId == 0L) {
      // If targetData is empty, generate IDs starting from 1
      newData.withColumn(idColumnName, monotonically_increasing_id() + 1)
    } else {
      // Generate IDs starting from MAX + 1
      val windowSpec = Window.partitionBy(col(uuidColumn)).orderBy(col(datetimeColumn).asc)
      val newWithIds = newData.withColumn(idColumnName, row_number().over(windowSpec) + lit(maxId))
      newWithIds
    }
    withIds
  }
}
