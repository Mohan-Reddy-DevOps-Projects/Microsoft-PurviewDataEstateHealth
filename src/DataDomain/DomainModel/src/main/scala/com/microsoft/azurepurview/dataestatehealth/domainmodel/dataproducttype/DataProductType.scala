package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproducttype

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions.{col, expr, lit, when}
import org.apache.spark.sql.types.{IntegerType, StringType}


class DataProductType (spark: SparkSession, logger:Logger){
  def processDataProductType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(col("payload.after.type").alias("DataProductTypeDisplayName")
        ,col("payload.after.type").alias("DataProductTypeDescription"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("payload.before.type").alias("DataProductTypeDisplayName")
          , col("payload.after.type").alias("DataProductTypeDescription"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else {
      dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter("DataProductTypeDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataProductTypeDisplayName"),"DataProductTypeID")

      dfProcess = dfProcess.select(col("DataProductTypeID").cast(StringType)
        ,col("DataProductTypeDisplayName").cast(StringType)
        ,col("DataProductTypeDescription").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductType Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductType Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataProductType"
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
                expr("target.DataProductTypeID = source.DataProductTypeID")
              )
              .whenNotMatched()
              .insertAll()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataProductType Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            df.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataProductType Does not exist for incremental merge. Performing Full Overwrite...")
          df.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataProductType data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataProductType data: ${e.getMessage}")
        throw e
    }
  }
}
