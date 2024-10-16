package com.microsoft.azurepurview.dataestatehealth.storagesync.common
import org.apache.log4j.Logger

class LakeCopy(logger:Logger){

  def processLakehouseCopy(sourceDataLakePath:String, FabricSyncRootPath:String):Boolean={
    try {
      mssparkutils.fs.rm(FabricSyncRootPath, recurse = true)
      mssparkutils.fs.cp(sourceDataLakePath, FabricSyncRootPath, true)
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
