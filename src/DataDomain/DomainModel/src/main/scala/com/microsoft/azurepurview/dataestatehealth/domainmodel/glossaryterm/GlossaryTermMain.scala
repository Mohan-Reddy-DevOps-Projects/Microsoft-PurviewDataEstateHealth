package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import com.microsoft.azurepurview.dataestatehealth.domainmodel.glossarytermbusinessdomainassignment.{GlossaryTermBusinessDomainAssignment, GlossaryTermBusinessDomainAssignmentSchema}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

object GlossaryTermMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In GlossaryTerm Main Main Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the GlossaryTerm Main Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn("term")) {
        val adlsTargetDirectory = args(0)
        val accountId = args(1)
        val refreshType = args(2)
        val jobRunGuid = args(3)
        println(
          s"""Received parameters: Target ADLS Path - $adlsTargetDirectory
        , AccountId - $accountId
        , Processing Type - $refreshType
        , JobRunGuid - $jobRunGuid""")
        val glossaryTermContractSchema = new GlossaryTermContractSchema().glossarytermContractSchema
        val glossaryTermSchema = new GlossaryTermSchema().glossaryTermSchema
        val glossaryTerm = new GlossaryTerm(spark, logger)
        val reader = new Reader(spark, logger)
        val df_glossaryTerm = reader.readCosmosData(glossaryTermContractSchema,"", accountId, "term", "DataCatalog", "Term")
        val df_glossaryTermProcessed = glossaryTerm.processGlossaryTerm(df_glossaryTerm, glossaryTermSchema)

        // Create The GlossaryTerm and BusinessDomain relationship Assignment - GlossaryTermBusinessDomainAssignment
        val glossaryTermBusinessDomainAssignmentSchema = new GlossaryTermBusinessDomainAssignmentSchema().glossaryTermBusinessDomainAssignmentSchema
        val glossaryTermBusinessDomainAssignment = new GlossaryTermBusinessDomainAssignment(spark, logger)
        val df_glossaryTermBusinessDomainAssignmentProcessed = glossaryTermBusinessDomainAssignment.processGlossaryTermBusinessDomainAssignment(df_glossaryTermProcessed, glossaryTermBusinessDomainAssignmentSchema, adlsTargetDirectory)
        val dataWriter = new DataWriter(spark)
        dataWriter.writeData(df_glossaryTermBusinessDomainAssignmentProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "GlossaryTermBusinessDomainAssignment")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/GlossaryTermBusinessDomainAssignment"), Some(df_glossaryTermBusinessDomainAssignmentProcessed), jobRunGuid, "GlossaryTermBusinessDomainAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTermBusinessDomainAssignment"))
        val finalDf = df_glossaryTermProcessed.drop("BusinessDomainId")
        dataWriter.writeData(finalDf, adlsTargetDirectory, ReProcessingThresholdInMins
          , "GlossaryTerm", Seq("GlossaryTermID"), refreshType)
        // GlossaryTermBusinessDomainAssignment Relationship is built, write the GlossaryTerm
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/GlossaryTerm"), Some(finalDf), jobRunGuid, "GlossaryTerm", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTerm"))

      }
    } catch {
      case e: Exception =>
        println(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/GlossaryTerm"), None, args(4), "GlossaryTerm", s"Error In Main GlossaryTerm Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
    }
  }
}
