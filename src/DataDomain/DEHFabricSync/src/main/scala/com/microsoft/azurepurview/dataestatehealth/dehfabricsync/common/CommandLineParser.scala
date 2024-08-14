package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common
import scopt.OParser
class CommandLineParser {
  val builder = OParser.builder[MainConfig]
  val parser = {
    import builder._
    OParser.sequence(
      programName("DEHFabricSync"),
      head("DEHFabricSync", "1.0"),
      opt[String]("DEHStorageAccount")
        .action((x, c) => c.copy(DEHStorageAccount = x))
        .text("DEH Tenant AccountId Storage"),
      opt[String]("FabricSyncRootPath")
        .action((x, c) => c.copy(FabricSyncRootPath = x))
        .text("Target Fabric Lakehouse Root Path"),
      opt[String]("AccountId")
        .action((x, c) => c.copy(AccountId = x))
        .text("AccountId"),
    opt[Boolean]("ProcessDomainModel")
      .action((x, c) => c.copy(ProcessDomainModel = x))
      .text("ProcessDomainModel"),
    opt[Boolean]("ProcessDimensionaModel")
      .action((x, c) => c.copy(ProcessDimensionalModel = x))
      .text("ProcessDimensionaModel"),
      opt[String]("JobRunGuid")
        .action((x, c) => c.copy(JobRunGuid = x))
        .text("JobRunGuid"))
  }
  def parse(args: Array[String]): Option[MainConfig] = {
    OParser.parse(parser, args, MainConfig())
  }
}
