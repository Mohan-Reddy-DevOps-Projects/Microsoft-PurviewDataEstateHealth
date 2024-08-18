package com.microsoft.azurepurview.dataestatehealth.storagesync.common
import scopt.OParser
class CommandLineParser {
  val builder = OParser.builder[MainConfig]
  val parser = {
    import builder._
    OParser.sequence(
      programName("StorageSync"),
      head("StorageSync", "1.0"),
      opt[String]("DEHStorageAccount")
        .action((x, c) => c.copy(DEHStorageAccount = x))
        .text("DEH Tenant AccountId Storage"),
      opt[String]("SyncRootPath")
        .action((x, c) => c.copy(SyncRootPath = x))
        .text("Target Lake Root Path"),
      opt[String]("SyncType")
        .action((x, c) => c.copy(SyncType = x))
        .text("Target Type"),
      opt[String]("AccountId")
        .action((x, c) => c.copy(AccountId = x))
        .text("AccountId"),
      opt[String]("JobRunGuid")
        .action((x, c) => c.copy(JobRunGuid = x))
        .text("JobRunGuid"))
  }
  def parse(args: Array[String]): Option[MainConfig] = {
    OParser.parse(parser, args, MainConfig())
  }
}
