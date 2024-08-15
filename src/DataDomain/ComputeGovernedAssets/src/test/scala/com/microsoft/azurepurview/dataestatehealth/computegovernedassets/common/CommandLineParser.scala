package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.common

import scopt.OParser
class CommandLineParser {
  val builder = OParser.builder[MainConfig]
  val parser = {
    import builder._
    OParser.sequence(
      programName("TestDomainModelMain"),
      head("TestDomainModelMain", "1.0"),
      opt[String]("AdlsTargetDirectory")
        .action((x, c) => c.copy(AdlsTargetDirectory = x))
        .text("Target ADLS Path")
    )
  }

  def parse(args: Array[String]): Option[MainConfig] = {
    OParser.parse(parser, args, MainConfig())
  }
}

