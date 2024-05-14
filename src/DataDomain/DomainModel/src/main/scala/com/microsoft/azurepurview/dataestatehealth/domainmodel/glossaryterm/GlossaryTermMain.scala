package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{ColdStartSoftCheck, Maintenance, Reader, Writer}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.glossarytermbusinessdomainassignment.{GlossaryTermBusinessDomainAssignment, GlossaryTermBusinessDomainAssignmentSchema}
import org.apache.spark.sql.SparkSession
import org.apache.log4j.{Level, Logger}
object GlossaryTermMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String],spark:SparkSession,ReProcessingThresholdInMins:Int): Unit = {
    try{
      println("In GlossaryTerm Main Main Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the GlossaryTerm Main Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark,logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0),"term")) {
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
        val glossaryTermContractSchema = new GlossaryTermContractSchema().glossarytermContractSchema
        val glossaryTermSchema = new GlossaryTermSchema().glossaryTermSchema
        val glossaryTerm = new GlossaryTerm(spark,logger)
        val reader = new Reader(spark,logger)
        val df_glossaryTerm = reader.readCosmosData(glossaryTermContractSchema,CosmosDBLinkedServiceName, accountId,"term","DataCatalog","Term")
        val df_glossaryTermProcessed = glossaryTerm.processGlossaryTerm(df_glossaryTerm,glossaryTermSchema)

        // Create The GlossaryTerm and BusinessDomain relationship Assignment - GlossaryTermBusinessDomainAssignment
        val glossaryTermBusinessDomainAssignmentSchema = new GlossaryTermBusinessDomainAssignmentSchema().glossaryTermBusinessDomainAssignmentSchema
        val glossaryTermBusinessDomainAssignment = new GlossaryTermBusinessDomainAssignment(spark, logger)
        val df_glossaryTermBusinessDomainAssignmentProcessed = glossaryTermBusinessDomainAssignment.processGlossaryTermBusinessDomainAssignment(df_glossaryTermProcessed,glossaryTermBusinessDomainAssignmentSchema,adlsTargetDirectory)
        val writer = new Writer(logger)
        writer.overWriteData(df_glossaryTermBusinessDomainAssignmentProcessed,adlsTargetDirectory,"GlossaryTermBusinessDomainAssignment",ReProcessingThresholdInMins)
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/GlossaryTermBusinessDomainAssignment"),Some(df_glossaryTermBusinessDomainAssignmentProcessed),jobRunGuid, "GlossaryTermBusinessDomainAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTermBusinessDomainAssignment"))

        // GlossaryTermBusinessDomainAssignment Relationship is built, write the GlossaryTerm
        glossaryTerm.writeData(df_glossaryTermProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/GlossaryTerm"),Some(df_glossaryTermProcessed),jobRunGuid, "GlossaryTerm","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/GlossaryTerm"))

      }
    } catch
    {
      case e: Exception =>
        println(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(args(2),args(1).concat("/GlossaryTerm"),None,args(4), "GlossaryTerm",s"Error In Main GlossaryTerm Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main GlossaryTerm Main Spark Application!: ${e.getMessage}")
    }
  }
}
