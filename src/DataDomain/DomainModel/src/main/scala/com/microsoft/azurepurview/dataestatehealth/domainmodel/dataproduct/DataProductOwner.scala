package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{coalesce, col, explode_outer, lit, row_number}
import org.apache.spark.sql.types.{StringType, TimestampType}

import java.sql.Timestamp

class DataProductOwner (spark: SparkSession, logger:Logger){
  def processDataProductOwner(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(col("payload.after.id").alias("DataProductId")
        ,col("payload.after.contacts.owner").alias("Owner")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("payload.before.id").alias("DataProductId")
          ,col("payload.before.contacts.owner").alias("Owner")
          ,col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }

      val windowSpecDataProduct = Window.partitionBy("DataProductId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpecDataProduct))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess
        .drop("ModifiedDateTime")

      dfProcess = dfProcess
        .withColumn("afterExplodeOwner", explode_outer(col("Owner")))
        .select(col("DataProductId")
        ,col("afterExplodeOwner.id").alias("DataProductOwnerId")
        ,col("afterExplodeOwner.description").alias("DataProductOwnerDescription"))
        .distinct()

      dfProcess = dfProcess.filter(s"""DataProductId IS NOT NULL
                                      | AND DataProductOwnerId IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("DataProductId").cast(StringType)
        ,col("DataProductOwnerId").cast(StringType)
        ,col("DataProductOwnerDescription").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataProductId is null
                            | or DataProductOwnerId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductOwner Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductOwner Data: ${e.getMessage}")
        throw e
    }
  }
}
