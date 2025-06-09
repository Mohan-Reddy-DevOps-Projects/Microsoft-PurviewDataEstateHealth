package com.microsoft.azurepurview.dataestatehealth.controls.common

import scopt.OParser

/**
 * Parser for command line arguments using scopt library.
 * 
 * This class provides a clean interface for parsing command-line arguments
 * into the MainConfig case class.
 */
object CommandLineParser {
  private val builder = OParser.builder[MainConfig]
  
  /**
   * The parser configuration defining all supported command-line options
   */
  private val parser = {
    import builder._
    OParser.sequence(
      programName("ControlsMain"),
      head("ControlsMain", "1.0"),
      
      // For backward compatibility, we keep the command-line parameter names with
      // PascalCase but map them to camelCase in the MainConfig case class
      opt[String]("AdlsTargetDirectory")
        .action((x, c) => c.copy(adlsTargetDirectory = x))
        .text("Target ADLS Path"),
      
      opt[Int]("ReProcessingThresholdInMins")
        .action((x, c) => c.copy(reProcessingThresholdInMins = x))
        .text("Re-processing Threshold (in minutes)")
        .validate(x => 
          if (x >= 0) success 
          else failure("ReProcessingThresholdInMins must be >= 0")),
      
      opt[String]("AccountId")
        .action((x, c) => c.copy(accountId = x))
        .text("Account ID"),
      
      opt[String]("RefreshType")
        .action((x, c) => c.copy(refreshType = x))
        .text("Refresh Type (Full or Incremental)")
        .validate(x => 
          if (x == "Full" || x == "Incremental") success
          else failure("RefreshType must be 'Full' or 'Incremental'")),
      
      opt[String]("JobRunGuid")
        .action((x, c) => c.copy(jobRunGuid = x))
        .text("Unique JobRunGuid (CorrelationId)"),
      
      opt[String]("TenantId")
        .action((x, c) => c.copy(tenantId = x))
        .text("Tenant ID")
    )
  }

  /**
   * Parse command line arguments into a MainConfig object
   *
   * @param args Command line arguments to parse
   * @return The parsed MainConfig or default MainConfig if parsing fails
   * @throws IllegalArgumentException if required parameters are missing or args is empty
   */
  def parseArgs(args: Array[String]): MainConfig = {
    // First check if args is empty and throw an exception if it is
    if (args.isEmpty) {
      throw new IllegalArgumentException("Failed to parse command line arguments: empty arguments array")
    }
    
    OParser.parse(parser, args, MainConfig()) match {
      case Some(config) => config
      case None => 
        throw new IllegalArgumentException("Failed to parse command line arguments")
    }
  }
}