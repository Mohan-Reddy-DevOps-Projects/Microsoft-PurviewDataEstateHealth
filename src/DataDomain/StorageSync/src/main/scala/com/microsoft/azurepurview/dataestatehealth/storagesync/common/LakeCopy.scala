package com.microsoft.azurepurview.dataestatehealth.storagesync.common

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging
import org.apache.log4j.Logger

class LakeCopy() extends SparkLogging {

  def processLakehouseCopy(sourceDataLakePath: String, FabricSyncRootPath: String): Boolean = {
    try {
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

      true
    } catch {
      case e: Exception =>
        logger.error(
          s"""An error occurred: when copying
             | From - $sourceDataLakePath,
             | To - $FabricSyncRootPath,
             | Errored with - ${e.getMessage}""".stripMargin)
        false
    }
  }
}
