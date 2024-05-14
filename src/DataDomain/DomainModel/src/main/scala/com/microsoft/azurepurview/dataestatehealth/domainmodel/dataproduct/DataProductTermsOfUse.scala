package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{GenerateId, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, explode_outer, lit, lower, row_number, trim, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}

import java.sql.Timestamp

class DataProductTermsOfUse (spark: SparkSession, logger:Logger){
  def processDataProductTermsOfUse(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(
        col("payload.after.id").alias("DataProductId"),
        col("payload.after.termsOfUse").alias("termsOfUse"),
        col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
      ).filter("operationType=='Create' or operationType=='Update'")

      val deleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!deleteIsEmpty) {
        val dfProcessDelete = df.select(
          col("payload.before.id").alias("DataProductId"),
          col("payload.before.termsOfUse").alias("termsOfUse"),
          col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime")
        ).filter("operationType=='Delete'")
        dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
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

      dfProcess = dfProcess.withColumn("termsOfUse_Exploded",explode_outer(col("termsOfUse")))
      dfProcess = dfProcess.select(
        col("DataProductId"),
        col("termsOfUse_Exploded.name").alias("TermsOfUseDisplayName"),
        col("termsOfUse_Exploded.url").alias("TermsOfUseHyperlink"),
        col("termsOfUse_Exploded.dataAssetId").alias("DataAssetId")
      )

      dfProcess = dfProcess.filter(s"""DataProductId IS NOT NULL
                                      | AND TermsOfUseDisplayName IS NOT NULL
                                      | AND TermsOfUseHyperlink IS NOT NULL
                                      |  AND DataAssetId IS NOT NULL""".stripMargin).distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataProductId","TermsOfUseDisplayName","TermsOfUseHyperlink","DataAssetId"),"TermsOfUseId")

      dfProcess = dfProcess.select(col("DataProductID").cast(StringType)
        ,col("TermsOfUseId").cast(StringType)
        ,col("TermsOfUseDisplayName").alias("TermsOfUseDisplayName").cast(StringType)
        ,col("TermsOfUseHyperlink").alias("TermsOfUseHyperlink").cast(StringType)
        ,col("DataAssetId").alias("DataAssetId").cast(StringType)
      )

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataProductID is null
                            | or TermsOfUseId is null
                            | or TermsOfUseDisplayName is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductTermsOfUse Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductTermsOfUse Data: ${e.getMessage}")
        throw e
    }
  }
}
