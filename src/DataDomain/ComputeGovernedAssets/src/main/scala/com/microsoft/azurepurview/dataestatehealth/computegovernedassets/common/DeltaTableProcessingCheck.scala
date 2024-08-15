package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.common
import io.delta.tables.DeltaTable
class DeltaTableProcessingCheck(adlsTargetDirectory: String) {
  def isDeltaTableRefreshedWithinXMinutes(Entity: String,ReProcessingThresholdInMins:Int): Boolean = {
    val Minutes=ReProcessingThresholdInMins
    /*
    We will revisit this to make it metadata driven,
    The idea is to NOT PROCESS or SKIP processing the entities in the model
    which have been processed in last X Minutes, this will help with reruns.
    And re-processing data each time when there is re-try upon failure.
    For now the check has been included at a single point i.e. Writer Class.
    Ultimately it will make it into the config based framework.
     */
    val directoryPath = adlsTargetDirectory.concat("/").concat(Entity)
    try {
      if (DeltaTable.isDeltaTable(directoryPath)) {
        val metadata = DeltaTable.forPath(directoryPath).history(1).select("timestamp").collect()(0).getTimestamp(0)
        val currentDateTime = java.sql.Timestamp.valueOf(java.time.LocalDateTime.now())
        val timeDifferenceMinutes = java.time.Duration.between(metadata.toLocalDateTime, currentDateTime.toLocalDateTime).toMinutes
        timeDifferenceMinutes <= Minutes
      } else {
        false
      }
    } catch {
      case _: Throwable => false
    }
  }
}
