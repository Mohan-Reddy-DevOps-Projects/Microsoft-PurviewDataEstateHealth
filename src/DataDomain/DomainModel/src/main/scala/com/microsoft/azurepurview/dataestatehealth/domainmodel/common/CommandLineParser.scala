package com.microsoft.azurepurview.dataestatehealth.domainmodel.common
import scopt.OParser
class CommandLineParser {
  val builder = OParser.builder[MainConfig]
  val parser = {
    import builder._
    OParser.sequence(
      programName("DomainModelMain"),
      head("DomainModelMain", "1.0"),
      opt[String]("CosmosDBLinkedServiceName")
        .action((x, c) => c.copy(CosmosDBLinkedServiceName = x))
        .text("Source Cosmos Linked Service"),
      opt[String]("AdlsTargetDirectory")
        .action((x, c) => c.copy(AdlsTargetDirectory = x))
        .text("Target ADLS Path"),
      opt[String]("AccountId")
        .action((x, c) => c.copy(AccountId = x))
        .text("Account ID"),
      opt[String]("RefreshType")
        .action((x, c) => c.copy(RefreshType = x))
        .text("Refresh Type"),
      opt[Int]("ReProcessingThresholdInMins")
        .action((x, c) => c.copy(ReProcessingThresholdInMins = x))
        .text("Re-processing Threshold (in minutes)"),
      opt[String]("JobRunGuid")
        .action((x, c) => c.copy(JobRunGuid = x))
        .text("Unique JobRunGuid (CorrelationId)")
    )
  }

  def parse(args: Array[String]): Option[MainConfig] = {
    OParser.parse(parser, args, MainConfig())
  }
}
