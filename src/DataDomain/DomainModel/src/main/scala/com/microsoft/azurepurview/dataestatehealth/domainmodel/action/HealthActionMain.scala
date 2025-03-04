package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

package object HealthActionMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In Action Main Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the HealthAction Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn("DHAction", "dgh-Action")) {
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
        val tenantId = spark.conf.get("spark.purview.tenantId", "")

        val actionContractSchema = new HealthActionContractSchema().healthActionContractSchema
        val actionSchema = new HealthActionSchema().healthActionSchema
        val action = new HealthAction(spark, logger)
        val dfAction = reader.readCosmosData(actionContractSchema,"", accountId, "DHAction", "","","dgh-Action",tenantId)

        val healthActionFindingTypeSchema = new HealthActionFindingTypeSchema().healthActionFindingTypeSchema
        val healthActionFindingType = new HealthActionFindingType(spark, logger)
        val df_healthActionFindingTypeProcessed = healthActionFindingType.processDataFindingType(dfAction, healthActionFindingTypeSchema)
        dataWriter.writeData(df_healthActionFindingTypeProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "HealthActionFindingType", Seq("FindingTypeId"), refreshType, "InsertOnly")

        val healthActionFindingSubTypeSchema = new HealthActionFindingSubTypeSchema().healthActionFindingSubTypeSchema
        val healthActionFindingSubType = new HealthActionFindingSubType(spark, logger)
        val df_healthActionFindingSubTypeProcessed = healthActionFindingSubType.processDataFindingSubType(dfAction, healthActionFindingSubTypeSchema, adlsTargetDirectory)
        dataWriter.writeData(df_healthActionFindingSubTypeProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "HealthActionFindingSubType", Seq("FindingSubTypeId"), refreshType, "InsertOnly")

        val healthActionUserAssignmentSchema = new HealthActionUserAssignmentSchema().healthActionUserAssignmentSchema
        val healthActionUserAssignment = new HealthActionUserAssignment(spark, logger)
        val df_healthActionUserAssignmentProcessed = healthActionUserAssignment.processHealthActionUserAssignment(dfAction, healthActionUserAssignmentSchema)
        dataWriter.writeData(df_healthActionUserAssignmentProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "HealthActionUserAssignment")

        val dfActionProcessed = action.processAction(dfAction, actionSchema, adlsTargetDirectory)
        dataWriter.writeData(dfActionProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "HealthAction", Seq("ActionId"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main HealthAction Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main HealthAction Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/Action"), None, args(4), "Action"
          , s"Error In Main HealthAction Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main HealthAction Spark Application!: ${e.getMessage}")
    }
  }
}
