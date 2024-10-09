package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductupdatefrequency
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, expr, lit, when}
import org.apache.spark.sql.types.{IntegerType, StringType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class DataProductUpdateFrequency (spark: SparkSession, logger:Logger){
  def processDataProductUpdateFrequency(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("payload.after.updateFrequency").alias("UpdateFrequencyDisplayName"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
      val dfProcessDelete = df.select(col("payload.before.updateFrequency").alias("UpdateFrequencyDisplayName"))
        .filter("operationType=='Delete'")
      dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter("UpdateFrequencyDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("UpdateFrequencyDisplayName"),"UpdateFrequencyID")

      dfProcess = dfProcess.select(col("UpdateFrequencyID").cast(StringType)
        ,col("UpdateFrequencyDisplayName").cast(StringType))

      dfProcess = dfProcess.filter(s"""UpdateFrequencyID IS NOT NULL
                                      | AND UpdateFrequencyDisplayName IS NOT NULL""".stripMargin).distinct()

      dfProcess=dfProcess.withColumn("SortOrder", lit(null: IntegerType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductUpdateFrequency Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductUpdateFrequency Data: ${e.getMessage}")
        throw e
    }
  }
}
