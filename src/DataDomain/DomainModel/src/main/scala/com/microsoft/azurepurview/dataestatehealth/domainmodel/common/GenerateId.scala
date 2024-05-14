package com.microsoft.azurepurview.dataestatehealth.domainmodel.common
import org.apache.spark.sql.functions._
import org.apache.spark.sql.DataFrame
import java.nio.charset.StandardCharsets
import java.util.UUID
class GenerateId {
  def IdGenerator(df: DataFrame, columns: List[String], targetColumnName: String): DataFrame = {
    // Select the specified columns and cast them to string
    val selectedColumns = columns.map(colName => rtrim(ltrim(col(colName).cast("string"))))
    // Replace null values with "~" in each selected column
    val replacedColumns = selectedColumns.map(col => when(col.isNull, lit("~")).otherwise(col).alias(col.toString))
    // Concatenate all values into a single column
    val concatenatedColumn = concat(replacedColumns: _*).as(targetColumnName)
    // Add the concatenated and hashed column to the DataFrame
    val uuidUDF = udf((str: String) => UUID.nameUUIDFromBytes(str.getBytes(StandardCharsets.UTF_8)).toString)
    var resultDF = df.withColumn(targetColumnName, uuidUDF(concatenatedColumn))
    //var resultDF = df.withColumn(targetColumnName, sha2(concatenatedColumn, 256))
    val orderedColumns = targetColumnName +: resultDF.columns.filterNot(_ == targetColumnName)
    // Add the targetColumnName to the DataFrame and order it as first
    resultDF = resultDF.select(orderedColumns.map(col): _*)
    resultDF
  }
  def generateUUID(str: String): String = {
    if (str.isEmpty) UUID.nameUUIDFromBytes("~".getBytes(StandardCharsets.UTF_8)).toString else UUID.nameUUIDFromBytes(str.getBytes(StandardCharsets.UTF_8)).toString
  }
}