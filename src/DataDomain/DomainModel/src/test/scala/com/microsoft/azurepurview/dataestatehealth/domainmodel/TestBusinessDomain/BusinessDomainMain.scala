package com.microsoft.azurepurview.dataestatehealth.domainmodel.TestBusinessDomain
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object BusinessDomainMain {
  val logger : Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String],spark:SparkSession): Unit = {
    println("In Test BusinessDomain Table Main Spark Application!")
    logger.setLevel(Level.INFO)
    logger.info("Started the Test BusinessDomain Table Main Spark Application!")

    if (args.length >= 1) {
      val adlsTargetDirectory = args(0)
      println(s"Received parameter: Target Path - $adlsTargetDirectory")

      val DomainTable = new BusinessDomain(spark,logger,adlsTargetDirectory).testBusinessDomainTable()

    } else {
      logger.error("Insufficient parameters. Please provide 2 parameters. Source Path and Target Path.")
      throw new IllegalArgumentException("Insufficient parameters. Please provide 2 parameters. Source Path and Target Path.")
    }
  }

}
