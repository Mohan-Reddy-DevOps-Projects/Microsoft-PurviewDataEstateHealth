package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

package object OKRMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In OKR Main Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the OKR Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn( "okr")) {
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

        val keyResultContractSchema = new KeyResultContractSchema().keyResultContractSchema
        val keyResultSchema = new KeyResultSchema().keyResultSchema
        val keyResult = new KeyResult(spark, logger)
        val dfKeyResult = reader.readCosmosData(keyResultContractSchema,"", accountId, "keyresult", "DataCatalog","KeyResult")
        val dfKeyResultProcessed = keyResult.processKeyResult(dfKeyResult, keyResultSchema, adlsTargetDirectory)
        dataWriter.writeData(dfKeyResultProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "KeyResult", Seq("KeyResultId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/KeyResult"), Some(dfKeyResultProcessed), jobRunGuid, "KeyResult", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/KeyResult"))

        val okrKeyResultAssignment = new OKRKeyResultAssignment(spark, logger)
        val okrKeyResultAssignmentSchema = new OKRKeyResultAssignmentSchema().okrKeyResultAssignmentSchema
        val dfOKRKeyResultAssignment = okrKeyResultAssignment.processOKRKeyResultAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = okrKeyResultAssignmentSchema)
        dataWriter.writeData(dfOKRKeyResultAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "OKRKeyResultAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/OKRKeyResultAssignment"), Some(dfOKRKeyResultAssignment), jobRunGuid, "OKRKeyResultAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/OKRKeyResultAssignment"))

        val dataProductOKRAssignment = new DataProductOKRAssignment(spark, logger)
        val dataProductOKRAssignmentSchema = new DataProductOKRAssignmentSchema().dataProductOKRAssignmentSchema
        val dfDataProductOKRAssignment = dataProductOKRAssignment.processDataProductOKRAssignment(
          adlsTargetDirectory = adlsTargetDirectory, schema = dataProductOKRAssignmentSchema)
        dataWriter.writeData(dfDataProductOKRAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataProductOKRAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataProductOKRAssignment"), Some(dfDataProductOKRAssignment), jobRunGuid, "DataProductOKRAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductOKRAssignment"))

        val okrContractSchema = new OKRContractSchema().okrContractSchema
        val okrSchema = new OKRSchema().okrSchema
        val okr = new OKR(spark, logger)
        val dfOkr = reader.readCosmosData(okrContractSchema, "", accountId, "okr", "DataCatalog", "OKR")
        val dfOkrProcessed = okr.processOKR(dfOkr, okrSchema)
        dataWriter.writeData(dfOkrProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "OKR", Seq("OKRId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/OKR"), Some(dfOkrProcessed), jobRunGuid, "OKR", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/OKR"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main OKR Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main OKR Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/OKR"), None, args(4), "OKR", s"Error In Main OKR Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main OKR Spark Application!: ${e.getMessage}")
    }
  }
}
