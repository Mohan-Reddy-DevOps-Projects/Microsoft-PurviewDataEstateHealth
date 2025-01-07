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
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "okr")) {
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

        val keyResultContractSchema = new KeyResultContractSchema().keyResultContractSchema
        val keyResultSchema = new KeyResultSchema().keyResultSchema
        val keyResult = new KeyResult(spark, logger)
        val dfKeyResult = reader.readCosmosData(keyResultContractSchema, CosmosDBLinkedServiceName, accountId, "keyresult", "DataCatalog","keyresult")
        val dfKeyResultProcessed = keyResult.processKeyResult(dfKeyResult, keyResultSchema)
        dataWriter.writeData(dfKeyResultProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "KeyResult", Seq("KeyResultId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/KeyResult"), Some(dfKeyResultProcessed), jobRunGuid, "KeyResult", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/KeyResult"))

        val okrContractSchema = new OKRContractSchema().okrContractSchema
        val okrSchema = new OKRSchema().okrSchema
        val okr = new OKR(spark, logger)
        val dfOkr = reader.readCosmosData(okrContractSchema, CosmosDBLinkedServiceName, accountId, "okr", "DataCatalog", "OKR")
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
