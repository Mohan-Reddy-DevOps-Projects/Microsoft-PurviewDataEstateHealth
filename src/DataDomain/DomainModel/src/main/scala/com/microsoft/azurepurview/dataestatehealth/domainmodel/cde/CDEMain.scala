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
      logger.info("Started the CDE Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "cde")) {
        val CosmosDBLinkedServiceName = args(0)
        val adlsTargetDirectory = args(1)
        val accountId = args(2)
        val refreshType = args(3)
        val jobRunGuid = args(4)
        println(
          s"""Received parameters: Source Cosmos Linked Service - $CosmosDBLinkedServiceName
        , Target ADLS Path - $adlsTargetDirectory
        , AccountId - $accountId
        , Processing Type - $refreshType
        , JobRunGuid - $jobRunGuid""")
        val reader = new Reader(spark, logger)
        val dataWriter = new DataWriter(spark)
        val VacuumOptimize = new Maintenance(spark, logger)

        val cdeContractSchema = new CDEContractSchema().cdeContractSchema
        val cdeSchema = new CDESchema().cdeSchema
        val cde = new CDE(spark, logger)
        val dfCDE = reader.readCosmosData(cdeContractSchema, CosmosDBLinkedServiceName, accountId, "cde", "DataCatalog","CriticalDataElement")
        val dfKeyResultProcessed = cde.processCDE(dfCDE, cdeSchema)
        dataWriter.writeData(dfKeyResultProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "CDE", Seq("CDEId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CDE"), Some(dfKeyResultProcessed), jobRunGuid, "CDE", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CDE"))

        val cdeDataProductAssignment = new CDEDataProductAssignment(spark, logger)
        val cdeDataProductAssignmentSchema = new CDEDataProductAssignmentSchema().cdeDataProductAssignmentSchema
        val dfCDEDataProductAssignment = cdeDataProductAssignment.processCDEDataProductAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeDataProductAssignmentSchema)
        dataWriter.writeData(dfCDEDataProductAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "CDEDataProductAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CDEDataProductAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "CDEDataProductAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CDEDataProductAssignment"))

        val cdeColumnAssignment = new CDEColumnAssignment(spark, logger)
        val cdeColumnAssignmentSchema = new CDEColumnAssignmentSchema().cdeColumnAssignmentSchema
        val dfCDEColumnAssignment = cdeColumnAssignment.processCDEColumnAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeColumnAssignmentSchema)
        dataWriter.writeData(dfCDEColumnAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "CDEColumnAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CDEColumnAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "CDEColumnAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CDEColumnAssignment"))

        val cdeGlossaryTermAssignment = new CDEGlossaryTermAssignment(spark, logger)
        val cdeGlossaryTermAssignmentSchema = new CDEGlossaryTermAssignmentSchema().cdeGlossaryTermAssignmentSchema
        val dfCDEGlossaryTermAssignment = cdeGlossaryTermAssignment.processCDEGlossaryTermAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = cdeGlossaryTermAssignmentSchema)
        dataWriter.writeData(dfCDEGlossaryTermAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "CDEGlossaryTermAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CDEGlossaryTermAssignment"), Some(dfCDEDataProductAssignment), jobRunGuid, "CDEGlossaryTermAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CDEGlossaryTermAssignment"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main CDE Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main CDE Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/CDE"), None, args(4), "CDE", s"Error In Main CDE Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main CDE Spark Application!: ${e.getMessage}")
    }
  }
}
