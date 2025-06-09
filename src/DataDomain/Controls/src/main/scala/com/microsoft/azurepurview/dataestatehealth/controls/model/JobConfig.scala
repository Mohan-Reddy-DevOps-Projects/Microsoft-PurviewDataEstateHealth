package com.microsoft.azurepurview.dataestatehealth.controls.model

/**
 * Configuration for Control Jobs processing
 *
 * @param adlsTargetDirectory The target directory in ADLS
 * @param accountId The account ID
 * @param refreshType The refresh type
 * @param jobRunGuid The job run GUID
 * @param reProcessingThresholdInMins The reprocessing threshold in minutes
 */
case class JobConfig(
  adlsTargetDirectory: String,
  accountId: String,
  refreshType: String,
  jobRunGuid: String,
  reProcessingThresholdInMins: Int
)

/**
 * Companion object for JobConfig
 */
object JobConfig {
  
  /**
   * Creates a JobConfig from command line arguments
   *
   * @param args The command line arguments
   * @param reProcessingThresholdInMins The reprocessing threshold in minutes
   * @return A new JobConfig
   */
  def fromArgs(args: Array[String], reProcessingThresholdInMins: Int): JobConfig = {
    val adlsTargetDirectory = if (args.length > 0) args(0) else ""
    val accountId = if (args.length > 1) args(1) else ""
    val refreshType = if (args.length > 2) args(2) else "Complete"
    val jobRunGuid = if (args.length > 3) args(3) else java.util.UUID.randomUUID().toString
    
    JobConfig(
      adlsTargetDirectory, 
      accountId, 
      refreshType, 
      jobRunGuid, 
      reProcessingThresholdInMins
    )
  }
} 