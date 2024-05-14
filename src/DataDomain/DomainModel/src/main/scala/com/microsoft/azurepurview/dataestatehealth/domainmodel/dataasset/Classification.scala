package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.GenerateId
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, expr, when}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types.{StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

import java.sql.Timestamp
class Classification (spark: SparkSession, logger:Logger){
  def processClassification(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      var dfProcess = df.select(col("payload.after.id").alias("DataAssetId")
        ,col("payload.after.schema.classifications").alias("Classification")
      ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime"))
        .filter("operationType=='Create' or operationType=='Update'")

      val windowSpec = Window
        .partitionBy("DataAssetId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess
        .drop("DataAssetId")
        .drop("ModifiedDateTime")

      dfProcess = dfProcess.withColumn("afterExplodeClassification", explode_outer(col("Classification")))
        .withColumn("DistinctClassification", explode_outer(col("afterExplodeClassification")))
        .select("DistinctClassification")
        .filter("DistinctClassification IS NOT NULL")
        .distinct()

      dfProcess = dfProcess.select(col("DistinctClassification").alias("ClassificationDisplayName")
      ,col("DistinctClassification").alias("ClassificationDescription"))

      dfProcess = dfProcess.filter(s"""ClassificationDisplayName IS NOT NULL
                                      | AND ClassificationDescription IS NOT NULL""".stripMargin).distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("ClassificationDisplayName"),"ClassificationId")

      dfProcess = dfProcess.select(col("ClassificationId").cast(StringType)
        ,col("ClassificationDisplayName").cast(StringType)
      ,col("ClassificationDescription").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing Classification Data: ${e.getMessage}")
        logger.error(s"Error Processing Classification Data: ${e.getMessage}")
        throw e
    }
  }
}
