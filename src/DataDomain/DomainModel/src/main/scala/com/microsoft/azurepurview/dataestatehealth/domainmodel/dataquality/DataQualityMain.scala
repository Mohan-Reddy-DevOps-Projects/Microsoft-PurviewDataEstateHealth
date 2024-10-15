package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object DataQualityMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In DataQuality Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the DataQuality Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0),"dataqualityv2fact")) {
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
        val dataQualityContractSchema = new DataQualityContractSchema().dataQualityContractSchema
        val dataQualityRuleSchema = new DataQualityRuleSchema().dataQualityRuleSchema
        val reader = new Reader(spark, logger)
        val df_dataQualityRule = reader.readCosmosData(dataQualityContractSchema,CosmosDBLinkedServiceName,accountId,"dataqualityv2fact","","dataQualityFact")
        val dataQualityRule = new DataQualityRule(spark,logger)
        //Process DataQualityRuleType
        val dataQualityRuleTypeSchema = new DataQualityRuleTypeSchema().dataQualityRuleTypeSchema
        val dataQualityRuleType = new DataQualityRuleType(spark, logger)
        val df_dataQualityRuleTypeProcessed = dataQualityRuleType.processDataQualityRuleType(df_dataQualityRule, dataQualityRuleTypeSchema)
        val dataWriter = new DataWriter(spark)
        dataWriter.writeData(df_dataQualityRuleTypeProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataQualityRuleType")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataQualityRuleType"), Some(df_dataQualityRuleTypeProcessed), jobRunGuid, "DataQualityRuleType", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataQualityRuleType"))

        //Process DataQualityJobExecution
        val dataQualityJobExecutionSchema = new DataQualityJobExecutionSchema().dataQualityJobExecutionSchema
        val dataQualityJobExecution = new DataQualityJobExecution(spark, logger)
        val df_dataQualityJobExecutionProcessed = dataQualityJobExecution.processDataQualityJobExecution(df_dataQualityRule, dataQualityJobExecutionSchema)
        dataWriter.writeData(df_dataQualityJobExecutionProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataQualityJobExecution", Seq("JobExecutionId")
          , refreshType, "InsertOnly")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataQualityJobExecution"), Some(df_dataQualityJobExecutionProcessed), jobRunGuid, "DataQualityJobExecution", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataQualityJobExecution"))

        //Process DataQualityRuleColumnExecution
        val dataQualityRuleColumnExecutionSchema = new DataQualityRuleColumnExecutionSchema().dataQualityRuleColumnExecutionSchema
        val dataQualityRuleColumnExecution = new DataQualityRuleColumnExecution(spark, logger)
        val df_dataQualityRuleColumnExecutionProcessed = dataQualityRuleColumnExecution.processDataQualityRuleColumnExecution(df_dataQualityRule, dataQualityRuleColumnExecutionSchema, adlsTargetDirectory)
        dataWriter.writeData(df_dataQualityRuleColumnExecutionProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataQualityRuleColumnExecution", Seq("JobExecutionId", "RuleId")
          , refreshType, "InsertOnly")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataQualityRuleColumnExecution"), Some(df_dataQualityRuleColumnExecutionProcessed), jobRunGuid, "DataQualityRuleColumnExecution", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataQualityRuleColumnExecution"))

        //Process DataQualityAssetRuleExecution
        val dataQualityAssetRuleExecutionSchema = new DataQualityAssetRuleExecutionSchema().dataQualityAssetRuleExecutionSchema
        val dataQualityAssetRuleExecution = new DataQualityAssetRuleExecution(spark, logger)
        val df_dataQualityAssetRuleExecutionProcessed = dataQualityAssetRuleExecution.processDataQualityAssetRuleExecution(df_dataQualityRule, dataQualityAssetRuleExecutionSchema)
        dataWriter.writeData(df_dataQualityAssetRuleExecutionProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataQualityAssetRuleExecution", Seq("JobExecutionId", "RuleId")
          , refreshType, "InsertOnly")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataQualityAssetRuleExecution"), Some(df_dataQualityAssetRuleExecutionProcessed), jobRunGuid, "DataQualityAssetRuleExecution", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataQualityAssetRuleExecution"))

        // Finally Process DataQualityRule
        val df_dataQualityRuleProcessed = dataQualityRule.processDataQualityRule(df_dataQualityRule, dataQualityRuleSchema)
        dataWriter.writeData(df_dataQualityRuleProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataQualityRule")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataQualityRule"), Some(df_dataQualityRuleProcessed), jobRunGuid, "DataQualityRule", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataQualityRule"))
      }
    } catch {
      case e: Exception =>
        println(s"Error In Main DataQuality Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main DataQuality Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/DataQualityJobExecution"), None, args(4), "DataQualityJobExecution", s"Error In Main DataQuality Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main DataQuality Spark Application!: ${e.getMessage}")
    }
  }
}
