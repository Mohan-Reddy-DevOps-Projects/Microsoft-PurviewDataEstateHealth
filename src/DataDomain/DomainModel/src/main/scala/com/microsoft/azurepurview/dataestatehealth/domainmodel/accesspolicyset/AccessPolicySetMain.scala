package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object AccessPolicySetMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In AccessPolicySet Main Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the AccessPolicySet Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "policyset")) {
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
        //Begin Processing AccessPolicySet
        val accessPolicySetContractSchema = new AccessPolicySetContractSchema().policySetContractSchema
        val accessPolicySetSchema = new AccessPolicySetSchema().accessPolicySetSchema
        val accessPolicySet = new AccessPolicySet(spark, logger)
        val reader = new Reader(spark, logger)
        val df_accessPolicySet = reader.readCosmosData(accessPolicySetContractSchema, CosmosDBLinkedServiceName, accountId, "policyset", "DataAccess", "PolicySet")
        val df_accessPolicySetProcessed = accessPolicySet.processAccessPolicySet(df_accessPolicySet, accessPolicySetSchema)
        // Process AccessPolicyProvisioningState
        val accessPolicyProvisioningStateSchema = new AccessPolicyProvisioningStateSchema().accessPolicyProvisioningStateSchema
        val accessPolicyProvisioningState = new AccessPolicyProvisioningState(spark, logger)
        val df_accessPolicyProvisioningStateProcessed = accessPolicyProvisioningState.processAccessPolicyProvisioningState(df_accessPolicySetProcessed, accessPolicyProvisioningStateSchema)
        val dataWriter = new DataWriter(spark)
        dataWriter.writeData(df_accessPolicyProvisioningStateProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "AccessPolicyProvisioningState")
        //writer.overWriteData(df_accessPolicyProvisioningStateProcessed,adlsTargetDirectory,"AccessPolicyProvisioningState",ReProcessingThresholdInMins)
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/AccessPolicyProvisioningState"), Some(df_accessPolicyProvisioningStateProcessed), jobRunGuid, "AccessPolicyProvisioningState", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/AccessPolicyProvisioningState"))
        //Process CustomAccessUseCase
        val customAccessUseCaseSchema = new CustomAccessUseCaseSchema().customAccessUseCaseSchema
        val customAccessUseCase = new CustomAccessUseCase(spark, logger)
        val df_customAccessUseCaseProcessed = customAccessUseCase.processCustomAccessUseCase(df_accessPolicySetProcessed, customAccessUseCaseSchema)
        dataWriter.writeData(df_customAccessUseCaseProcessed, adlsTargetDirectory,
          ReProcessingThresholdInMins, "CustomAccessUseCase")
        //writer.overWriteData(df_customAccessUseCaseProcessed,adlsTargetDirectory,"CustomAccessUseCase",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/CustomAccessUseCase"), Some(df_customAccessUseCaseProcessed), jobRunGuid, "CustomAccessUseCase", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/CustomAccessUseCase"))
        //Process AccessPolicyResourceType
        val accessPolicyResourceTypeSchema = new AccessPolicyResourceTypeSchema().accessPolicyResourceTypeSchema
        val accessPolicyResourceType = new AccessPolicyResourceType(spark, logger)
        val df_accessPolicyResourceTypeProcessed = accessPolicyResourceType.processAccessPolicyResourceType(df_accessPolicySetProcessed, accessPolicyResourceTypeSchema)
        dataWriter.writeData(df_accessPolicyResourceTypeProcessed, adlsTargetDirectory,
          ReProcessingThresholdInMins, "AccessPolicyResourceType")
        //writer.overWriteData(df_accessPolicyResourceTypeProcessed,adlsTargetDirectory,"AccessPolicyResourceType",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/AccessPolicyResourceType"), Some(df_accessPolicyResourceTypeProcessed), jobRunGuid, "AccessPolicyResourceType", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/AccessPolicyResourceType"))
        //Now Complete Processing For AccessPolicySet
        val finalDf = df_accessPolicySetProcessed
          .drop("ProvisioningStateDisplayName")
          .drop("PermittedUseCasesTitle")
          .drop("PermittedUseCasesDescription")
          .drop("ResourceType")
          .distinct()
        dataWriter.writeData(finalDf, adlsTargetDirectory, ReProcessingThresholdInMins
          , "AccessPolicySet", Seq("AccessPolicySetId"), refreshType)
        //accessPolicySet.writeData(df_accessPolicySetProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/AccessPolicySet"), Some(df_accessPolicySetProcessed), jobRunGuid, "AccessPolicySet", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/AccessPolicySet"))
      }
    } catch {
      case e: Exception =>
        println(s"Error In Main AccessPolicySet Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main AccessPolicySet Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/AccessPolicySet"), None, args(4), "AccessPolicySet", s"Error In Main AccessPolicySet Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main AccessPolicySet Spark Application!: ${e.getMessage}")
    }
  }
}
