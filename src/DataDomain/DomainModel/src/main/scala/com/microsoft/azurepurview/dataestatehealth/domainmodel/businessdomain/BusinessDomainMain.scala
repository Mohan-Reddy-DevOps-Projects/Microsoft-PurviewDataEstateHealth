package com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object BusinessDomainMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In Business Domain Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the Business Domain Table Main Application!")
      logger.warn("This is a warning message, after starting the main.")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)

      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn("businessdomain")) {
        val adlsTargetDirectory = args(0)
        val accountId = args(1)
        val refreshType = args(2)
        val jobRunGuid = args(3)
        println(
          s"""Received parameters: Target ADLS Path - $adlsTargetDirectory
        , AccountId - $accountId
        , Processing Type - $refreshType
        , JobRunGuid - $jobRunGuid""")
        val businessDomainContractSchema = new BusinessDomainContractSchema().businessDomainContractSchema
        val businessDomainSchema = new BusinessDomainSchema().businessDomainAssetSchema
        val businessDomain = new BusinessDomain(spark, logger)
        val dataWriter = new DataWriter(spark)
        val reader = new Reader(spark, logger)
        val df_domain = reader.readCosmosData(businessDomainContractSchema,"", accountId, "businessdomain", "DataCatalog", "BusinessDomain")
        val df_businessDomainProcessed = businessDomain.processBusinessDomain(df_domain, businessDomainSchema)
        // businessDomain.writeData(df_businessDomainProcessed,adlsTargetDirectory, refreshType,ReProcessingThresholdInMins)
        dataWriter.writeData(df = df_businessDomainProcessed, adlsTargetDirectory,
          ReProcessingThresholdInMins, "BusinessDomain", Seq("BusinessDomainId"), refreshType)
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/BusinessDomain"), Some(df_businessDomainProcessed), jobRunGuid, "BusinessDomain", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/BusinessDomain"))
      }
    } catch {
      case e: Exception =>
        println(s"Error In Main Business Domain Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main Business Domain Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/BusinessDomain"), None, args(4), "BusinessDomain", s"Error In Main BusinessDomain Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main Business Domain Spark Application!: ${e.getMessage}")
    }
  }
}
