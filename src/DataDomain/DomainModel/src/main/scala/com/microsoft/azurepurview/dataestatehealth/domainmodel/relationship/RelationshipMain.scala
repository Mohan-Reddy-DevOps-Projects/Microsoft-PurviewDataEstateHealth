package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object RelationshipMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In Relationship Main Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the Relationship Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0), "relationship")) {
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
        val relationshipContractSchema = new RelationshipContractSchema().relationshipContractSchema
        val relationshipSchema = new RelationshipSchema().relationshipSchema
        val relationship = new Relationship(spark, logger)
        val reader = new Reader(spark, logger)
        val df_relationship = reader.readCosmosData(relationshipContractSchema, CosmosDBLinkedServiceName, accountId, "relationship", "DataCatalog", "Relationship")
        val df_processRelationship = relationship.processRelationship(df_relationship, relationshipSchema)
        val dataWriter = new DataWriter(spark)
        dataWriter.writeData(df_processRelationship, adlsTargetDirectory
          , ReProcessingThresholdInMins, "Relationship")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/Relationship"), Some(df_processRelationship), jobRunGuid, "Relationship", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/Relationship"))
        //Process GlossaryTermDataProductAssignment
        val glossaryTermDataProductAssignmentSchema = new GlossaryTermDataProductAssignmentSchema().glossaryTermDataProductAssignmentSchema
        val glossaryTermDataProductAssignment = new GlossaryTermDataProductAssignment(spark, logger)
        val df_glossaryTermDataProductAssignmentProcessed = glossaryTermDataProductAssignment.processGlossaryTermBusinessDomainAssignment(glossaryTermDataProductAssignmentSchema, adlsTargetDirectory)
        dataWriter.writeData(df_glossaryTermDataProductAssignmentProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "GlossaryTermDataProductAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/GlossaryTermDataProductAssignment"), Some(df_glossaryTermDataProductAssignmentProcessed), jobRunGuid, "GlossaryTermDataProductAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTermDataProductAssignment"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main Relationship Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main Relationship Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/Relationship"), None, args(4), "Relationship", s"Error In Main Relationship Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main Relationship Spark Application!: ${e.getMessage}")
    }
  }
}
