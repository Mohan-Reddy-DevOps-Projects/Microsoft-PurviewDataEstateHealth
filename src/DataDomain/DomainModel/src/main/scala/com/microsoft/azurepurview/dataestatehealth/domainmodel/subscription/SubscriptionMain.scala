package com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object SubscriptionMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In Subscription Main Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the Subscription Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "datasubscription")) {
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
        //Begin Processing Subscription
        val subscriptionContractSchema = new SubscriptionContractSchema().subscriptionContractSchema
        val dataSubscriberRequestSchema = new DataSubscriberRequestSchema().dataSubscriberRequestSchema
        val reader = new Reader(spark, logger)
        val dataWriter = new DataWriter(spark)
        val df_subscription = reader.readCosmosData(subscriptionContractSchema, CosmosDBLinkedServiceName, accountId, "datasubscription", "DataAccess", "DataSubscription")
        val dataSubscriberRequest = new DataSubscriberRequest(spark, logger)
        val df_dataSubscriberRequestProcessed = dataSubscriberRequest.processDataSubscriberRequest(df_subscription, dataSubscriberRequestSchema)
        dataWriter.writeData(df_dataSubscriberRequestProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataSubscriberRequest"
          , Seq("SubscriberRequestId"), refreshType)
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataSubscriberRequest"), Some(df_dataSubscriberRequestProcessed), jobRunGuid, "DataSubscriberRequest", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataSubscriberRequest"))
        // Processing PolicySetApprover
        val policySetApproverSchema = new PolicySetApproverSchema().policySetApproverSchema
        val policySetApprover = new PolicySetApprover(spark, logger)
        val df_policySetApproverProcessed = policySetApprover.processPolicySetApprover(df_subscription, policySetApproverSchema)
        dataWriter.writeData(df_policySetApproverProcessed, adlsTargetDirectory, ReProcessingThresholdInMins
          , "PolicySetApprover", Seq("SubscriberRequestId"
            , "AccessPolicySetId", "ApproverIdentityType", "ApproverUserId", "ApproverUserTenantId"))
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/PolicySetApprover"), Some(df_policySetApproverProcessed), jobRunGuid, "PolicySetApprover", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/PolicySetApprover"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Subscription Main Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Subscription Main Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/Subscription"), None, args(4), "Subscription", s"Error In Main Subscription Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Subscription Main Spark Application!: ${e.getMessage}")
    }
  }
}
