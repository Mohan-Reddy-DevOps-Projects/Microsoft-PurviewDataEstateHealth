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
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataProductUpdateFrequency"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)){
      if (refreshType=="full"){
        df.write
          .format("delta")
          .mode("overwrite")
          .save(adlsTargetDirectory.concat("/").concat(EntityName))}
      else if (refreshType=="incremental") {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {

            dfTargetDeltaTable.as("target")
              .merge(
                df.as("source"),
                expr("target.UpdateFrequencyID = source.UpdateFrequencyID")
              )
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataProductUpdateFrequency Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataProductUpdateFrequency Does not exist for incremental merge. Performing Full Overwrite...")
          df.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataProductUpdateFrequency data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataProductUpdateFrequency data: ${e.getMessage}")
        throw e
    }
  }
}
