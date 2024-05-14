package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import io.delta.tables.DeltaTable
class Writer (logger:Logger){
  def overWriteData(df:DataFrame,adlsTargetDirectory:String,EntityName:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        val dfWrite = df.distinct()
        dfWrite.write
          .format("delta")
          .mode("overwrite")
          .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
      }
    }
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging $EntityName data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging $EntityName data: ${e.getMessage}")
        throw e
    }
  }
  def appendData(df:DataFrame,adlsTargetDirectory:String,EntityName:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        val tablePath = adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName)
        val targetDeltaTableExists = DeltaTable.isDeltaTable(tablePath)
        if (!targetDeltaTableExists) {
          df.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
        }
        else {
          df.write
            .format("delta")
            .mode("append")
            .save(adlsTargetDirectory.concat("/DimensionalModel/").concat(EntityName))
        }
      }
    }
    catch{
      case e: Exception =>
        println(s"Error Appending $EntityName data: ${e.getMessage}")
        logger.error(s"Error Appending $EntityName data: ${e.getMessage}")
        throw e
    }
  }
}
