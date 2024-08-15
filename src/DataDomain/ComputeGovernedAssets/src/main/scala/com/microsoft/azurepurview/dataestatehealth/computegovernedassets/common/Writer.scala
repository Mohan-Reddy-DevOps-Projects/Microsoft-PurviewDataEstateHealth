package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.common

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}

class Writer (logger:Logger){
  def overWriteData(df:DataFrame,adlsTargetDirectory:String,EntityName:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)) {
        val dfWrite = df.distinct()
        dfWrite.write
          .format("delta")
          .mode("overwrite")
          .save(adlsTargetDirectory.concat("/").concat(EntityName))
      }
    }
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging $EntityName data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging $EntityName data: ${e.getMessage}")
        throw e
    }
  }

}
