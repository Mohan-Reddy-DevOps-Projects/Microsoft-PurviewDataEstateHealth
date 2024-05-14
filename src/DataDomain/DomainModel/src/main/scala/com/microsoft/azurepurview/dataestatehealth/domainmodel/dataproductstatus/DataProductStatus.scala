package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductstatus
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, expr, when}
import org.apache.spark.sql.types.StringType
import org.apache.spark.sql.{DataFrame, SparkSession}

class DataProductStatus (spark: SparkSession, logger:Logger){

  def processDataProductStatus(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("payload.after.status").alias("DataProductStatusDisplayName"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("payload.before.status").alias("DataProductStatusDisplayName"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else {
      dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter("DataProductStatusDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataProductStatusDisplayName"),"DataProductStatusID")

      dfProcess = dfProcess.select(col("DataProductStatusID").cast(StringType)
        ,col("DataProductStatusDisplayName").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductStatus Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductStatus Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataProductStatus"
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
                expr("target.DataProductStatusID = source.DataProductStatusID")
              )
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataProductStatus Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataProductStatus Does not exist for incremental merge. Performing Full Overwrite...")
          df.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataProductStatus data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataProductStatus data: ${e.getMessage}")
        throw e
    }
  }

}
