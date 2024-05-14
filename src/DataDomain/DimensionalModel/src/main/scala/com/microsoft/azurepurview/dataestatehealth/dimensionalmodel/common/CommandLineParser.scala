package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import scopt.OParser

class CommandLineParser {
  val builder = OParser.builder[MainConfig]
  val parser = {
    import builder._
    OParser.sequence(
      programName("DimensionalModelMain"),
      head("DimensionalModelMain", "1.0"),
      opt[String]("AdlsTargetDirectory")
        .action((x, c) => c.copy(AdlsTargetDirectory = x))
        .text("Target ADLS Path"),
      opt[Int]("ReProcessingThresholdInMins")
        .action((x, c) => c.copy(ReProcessingThresholdInMins = x))
        .text("ReProcessingThresholdInMins"),
      opt[String]("AccountId")
        .action((x, c) => c.copy(AccountId = x))
        .text("AccountId"),
      opt[String]("JobRunGuid")
        .action((x, c) => c.copy(JobRunGuid = x))
        .text("Unique JobRunGuid (CorrelationId)")
    )
  }

  def parse(args: Array[String]): Option[MainConfig] = {
    OParser.parse(parser, args, MainConfig())
  }
}