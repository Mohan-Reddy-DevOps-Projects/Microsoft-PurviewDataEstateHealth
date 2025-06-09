package com.microsoft.azurepurview.dataestatehealth.controls.common

/**
 * Configuration class for command line parameters.
 * 
 * This contains all parameters needed for the Controls process. Properties follow
 * camelCase convention for consistency with Scala best practices.
 *
 * @param adlsTargetDirectory The target directory in ADLS to write results
 * @param accountId The account identifier
 * @param refreshType The type of refresh (Full or Incremental)
 * @param reProcessingThresholdInMins Threshold in minutes for reprocessing
 * @param jobRunGuid A unique identifier for this job run (correlation ID)
 * @param tenantId The tenant identifier
 */
case class MainConfig(
  adlsTargetDirectory: String = "",
  accountId: String = "",
  refreshType: String = "Full", // Default to Full refresh
  reProcessingThresholdInMins: Int = 0,
  jobRunGuid: String = "",
  tenantId: String = ""
)