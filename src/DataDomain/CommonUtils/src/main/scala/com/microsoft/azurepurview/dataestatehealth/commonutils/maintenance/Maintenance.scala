package com.microsoft.azurepurview.dataestatehealth.commonutils.maintenance

object Maintenance {
  def performMaintenance(adlsDir: String): Unit = {
    def fileExists(path: String): Boolean = {
      try {
        mssparkutils.fs.ls(path).nonEmpty
      } catch {
        case _: Exception => false
      }
    }
    if (fileExists(adlsDir)) {
      println("Delete root directory & full load.")
      mssparkutils.fs.rm(adlsDir, true)
    } else {
      println("No root directory.")
    }
  }
}
