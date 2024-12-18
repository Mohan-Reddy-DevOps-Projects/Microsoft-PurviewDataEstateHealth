package com.microsoft.azurepurview.dataestatehealth.storagesync.common

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging

class LakeCopy() extends SparkLogging {

  def processLakehouseCopy(sourceDataLakePath: String, FabricSyncRootPath: String): Unit = {
    def fileExists(path: String): Boolean = {
      try {
        mssparkutils.fs.ls(path).nonEmpty
      } catch {
        case _: Exception => false
      }
    }

    if (fileExists(FabricSyncRootPath)) {
      mssparkutils.fs.rm(FabricSyncRootPath, recurse = true)
    }

    mssparkutils.fs.cp(sourceDataLakePath, FabricSyncRootPath, true)

    logger.info(s"$sourceDataLakePath Copied successfully to $FabricSyncRootPath.")
    println(s"$sourceDataLakePath Copied successfully to $FabricSyncRootPath.")
  }
}
