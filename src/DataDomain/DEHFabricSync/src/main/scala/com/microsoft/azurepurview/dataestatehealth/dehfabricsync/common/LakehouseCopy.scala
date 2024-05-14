package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common
import org.apache.log4j.Logger
import org.apache.spark.sql.SparkSession

class LakehouseCopy (spark: SparkSession, logger:Logger){
  def processLakehouseCopy(sourceDataLakePath:String, FabricSyncRootPath:String):Boolean={
    try {
      mssparkutils.fs.fastcp(sourceDataLakePath, FabricSyncRootPath, true)
      println(s"$sourceDataLakePath Copied successfully to $FabricSyncRootPath.")
      true
    }catch {
      case e: Exception =>
        println(s"""An error occurred: when copying
                   | From - $sourceDataLakePath,
                   | To - $FabricSyncRootPath,
                   | Errored with - ${e.getMessage}""".stripMargin)
        false
    }
  }
}
