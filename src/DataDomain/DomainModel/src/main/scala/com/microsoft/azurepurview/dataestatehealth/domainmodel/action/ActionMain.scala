package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

package object ActionMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In Action Main Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the Action Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "DHAction","dgh-Action")) {
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
        val tenantId = spark.conf.get("spark.purview.tenantId", "")

        val actionContractSchema = new ActionContractSchema().actionContractSchema
        val actionSchema = new ActionSchema().actionSchema
        val action = new Action(spark, logger)
        val dfAction = reader.readCosmosData(actionContractSchema, CosmosDBLinkedServiceName
          , accountId, "DHAction", "","","dgh-Action",tenantId)
        val dfActionProcessed = action.processAction(dfAction, actionSchema)
        dataWriter.writeData(dfActionProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "Action", Seq("ActionId"))
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/Action"), Some(dfActionProcessed), jobRunGuid, "Action", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/Action"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main Action Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main Action Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/Action"), None, args(4), "Action"
          , s"Error In Main Action Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main Action Spark Application!: ${e.getMessage}")
    }
  }
}
