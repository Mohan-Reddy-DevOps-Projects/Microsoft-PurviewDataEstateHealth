package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

package object  CDEMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In CDE Main Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the CriticalDataElement Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn("cde")) {
        val adlsTargetDirectory = args(0)
        val accountId = args(1)
        val refreshType = args(2)
        val jobRunGuid = args(3)
        println(
          s"""Received parameters: Target ADLS Path - $adlsTargetDirectory
        , AccountId - $accountId
        , Processing Type - $refreshType
        , JobRunGuid - $jobRunGuid""")
        val reader = new Reader(spark, logger)
        val dataWriter = new DataWriter(spark)
        val VacuumOptimize = new Maintenance(spark, logger)

        val cdeContractSchema = new CDEContractSchema().cdeContractSchema
        val cdeSchema = new CDESchema().cdeSchema
        val cde = new CDE(spark, logger)
        val dfCDE = reader.readCosmosData(cdeContractSchema,"", accountId, "cde", "DataCatalog","CriticalDataElement")
        val dfCDEProcessed = cde.processCDE(dfCDE, cdeSchema)
        dataWriter.writeData(dfCDEProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "CriticalDataElement", Seq("CriticalDataElementId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CriticalDataElement"), Some(dfCDEProcessed), jobRunGuid, "CriticalDataElement", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CriticalDataElement"))

        val cdeDataProductAssignment = new CDEDataProductAssignment(spark, logger)
        val cdeDataProductAssignmentSchema = new CDEDataProductAssignmentSchema().cdeDataProductAssignmentSchema
        val dfCDEDataProductAssignment = cdeDataProductAssignment.processCDEDataProductAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeDataProductAssignmentSchema)
        dataWriter.writeData(dfCDEDataProductAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataProductCriticalDataElementAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataProductCriticalDataElementAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "DataProductCriticalDataElementAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductCriticalDataElementAssignment"))

        val cdeColumnAssignment = new CDEColumnAssignment(spark, logger)
        val cdeColumnAssignmentSchema = new CDEColumnAssignmentSchema().cdeColumnAssignmentSchema
        val dfCDEColumnAssignment = cdeColumnAssignment.processCDEColumnAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeColumnAssignmentSchema)

        dataWriter.writeData(dfCDEColumnAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetColumnCriticalDataElementAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetColumnCriticalDataElementAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "DataAssetColumnCriticalDataElementAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetColumnCriticalDataElementAssignment"))

        val cdeGlossaryTermAssignment = new CDEGlossaryTermAssignment(spark, logger)
        val cdeGlossaryTermAssignmentSchema = new CDEGlossaryTermAssignmentSchema().cdeGlossaryTermAssignmentSchema
        val dfCDEGlossaryTermAssignment = cdeGlossaryTermAssignment.processCDEGlossaryTermAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeGlossaryTermAssignmentSchema)

        dataWriter.writeData(dfCDEGlossaryTermAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "GlossaryTermCriticalDataElementAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/GlossaryTermCriticalDataElementAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "GlossaryTermCriticalDataElementAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTermCriticalDataElementAssignment"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main CriticalDataElement Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main CriticalDataElement Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main CriticalDataElement Spark Application!: ${e.getMessage}")
    }
  }
}
