package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.GenerateId
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, expr, lit, row_number, when}
import org.apache.spark.sql.types.{StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

import java.sql.Timestamp
class DataAssetType (spark: SparkSession, logger:Logger){
  def processDataAssetType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("payload.after.id").alias("DataAssetId")
        ,col("payload.after.type").alias("DataAssetTypeDisplayName")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime"))
        .filter("operationType=='Create' or operationType=='Update'")
      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
      val dfProcessDelete = df.select(col("payload.before.id").alias("DataAssetId")
      ,col("payload.before.type").alias("DataAssetTypeDisplayName")
      ,col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime"))
        .filter("operationType=='Delete'")
      dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }
      val windowSpec = Window.partitionBy("DataAssetId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess.filter("DataAssetTypeDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataAssetTypeDisplayName"),"DataAssetTypeId")

      dfProcess = dfProcess.select(col("DataAssetTypeId").cast(StringType)
        ,col("DataAssetTypeDisplayName").cast(StringType)).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetType Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetType Data: ${e.getMessage}")
        throw e
    }
  }
}