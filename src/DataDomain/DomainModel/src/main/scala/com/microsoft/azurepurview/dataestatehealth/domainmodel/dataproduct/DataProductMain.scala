package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{ColdStartSoftCheck, Maintenance, Reader, Writer}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductbuisnessdomainassignment.{DataProductBusinessDomainAssignment, DataProductBusinessDomainAssignmentSchema}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductstatus.{DataProductStatus, DataProductStatusSchema}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproducttype.{DataProductType, DataProductTypeSchema}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductupdatefrequency.{DataProductUpdateFrequency, DataProductUpdateFrequencySchema}
import org.apache.spark.sql.SparkSession
import org.apache.log4j.{Level, Logger}
object DataProductMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String],spark:SparkSession,ReProcessingThresholdInMins:Int): Unit = {
    try{
      println("In DataProduct Main Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the DataProduct Table Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark,logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0),"dataproduct")) {
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
        val dataProductContractSchema = new DataProductContractSchema().dataProductContractSchema
        val dataProductSchema = new DataProductSchema().dataProductAssetSchema
        val reader = new Reader(spark, logger)
        val dataProduct = new DataProduct(spark, logger)
        val df_dataProduct = reader.readCosmosData(dataProductContractSchema,CosmosDBLinkedServiceName, accountId,"dataproduct","DataCatalog","DataProduct")

        // Process DataProductStatus
        val dataProductStatusSchema = new DataProductStatusSchema().dataProductStatusSchema
        val dataProductStatus = new DataProductStatus(spark,logger)
        val df_dataProductStatusProcessed = dataProductStatus.processDataProductStatus(df_dataProduct,dataProductStatusSchema)
        dataProductStatus.writeData(df_dataProductStatusProcessed,adlsTargetDirectory, refreshType,ReProcessingThresholdInMins)
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductStatus"),Some(df_dataProductStatusProcessed),jobRunGuid, "DataProductStatus","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductStatus"))

        // Process DataProductUpdateFrequency
        val dataProductUpdateFrequencySchema = new DataProductUpdateFrequencySchema().dataProductUpdateFrequencySchema
        val dataProductUpdateFrequency = new DataProductUpdateFrequency(spark,logger)
        val df_dataProductUpdateFrequencyProcessed = dataProductUpdateFrequency.processDataProductUpdateFrequency(df_dataProduct,dataProductUpdateFrequencySchema)
        dataProductUpdateFrequency.writeData(df_dataProductUpdateFrequencyProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductUpdateFrequency"),Some(df_dataProductUpdateFrequencyProcessed),jobRunGuid, "DataProductUpdateFrequency","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductUpdateFrequency"))

        // Process DataProductType
        val dataProductTypeSchema = new DataProductTypeSchema().dataProductTypeSchema
        val dataProductType = new DataProductType(spark,logger)
        val df_dataProductTypeProcessed = dataProductType.processDataProductType(df_dataProduct,dataProductTypeSchema)
        dataProductType.writeData(df_dataProductTypeProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductType"),Some(df_dataProductTypeProcessed),jobRunGuid, "DataProductType","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductType"))

        // Process DataProductTermsOfUse
        val dataProductTermsOfUseSchema = new DataProductTermsOfUseSchema().dataProductTermsOfUseSchema
        val dataProductTermsOfUse = new DataProductTermsOfUse(spark,logger)
        val df_dataProductTermsOfUseProcessed = dataProductTermsOfUse.processDataProductTermsOfUse(df_dataProduct,dataProductTermsOfUseSchema)
        val writer = new Writer(logger)
        writer.overWriteData(df_dataProductTermsOfUseProcessed,adlsTargetDirectory,"DataProductTermsOfUse",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductTermsOfUse"),Some(df_dataProductTermsOfUseProcessed),jobRunGuid, "DataProductTermsOfUse","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductTermsOfUse"))

        // Process DataProductDocumentation
        val dataProductDocumentationSchema = new DataProductDocumentationSchema().dataProductDocumentationSchema
        val dataProductDocumentation = new DataProductDocumentation(spark,logger)
        val df_dataProductDocumentationProcessed = dataProductDocumentation.processDataProductDocumentation(df_dataProduct,dataProductDocumentationSchema)
        writer.overWriteData(df_dataProductDocumentationProcessed,adlsTargetDirectory,"DataProductDocumentation",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductDocumentation"),Some(df_dataProductDocumentationProcessed),jobRunGuid, "DataProductDocumentation","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductDocumentation"))

        // Process DataProduct
        val df_dataProductProcessed = dataProduct.processDataProduct(df_dataProduct,dataProductSchema,adlsTargetDirectory)

        // Create The DataProduct and BusinessDomain relationship Assignment - DataProductBusinessDomainAssignment
        val dataProductBusinessDomainAssignmentSchema = new DataProductBusinessDomainAssignmentSchema().dataProductBusinessDomainAssignmentSchema
        val dataProductBusinessDomainAssignment = new DataProductBusinessDomainAssignment(spark,logger)
        val df_dataProductBusinessDomainAssignmentProcessed = dataProductBusinessDomainAssignment.processDataProductBusinessDomainAssignment(df_dataProductProcessed,dataProductBusinessDomainAssignmentSchema,adlsTargetDirectory)
        writer.overWriteData(df_dataProductBusinessDomainAssignmentProcessed,adlsTargetDirectory,"DataProductBusinessDomainAssignment",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductBusinessDomainAssignment"),Some(df_dataProductBusinessDomainAssignmentProcessed),jobRunGuid, "DataProductBusinessDomainAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductBusinessDomainAssignment"))
       // Process DataProductOwner
        val dataProductOwnerSchema = new DataProductOwnerSchema().dataProductOwnerSchema
        val dataProductOwner = new DataProductOwner(spark,logger)
        val df_dataProductOwnerProcessed = dataProductOwner.processDataProductOwner(df_dataProduct,dataProductOwnerSchema)
        writer.overWriteData(df_dataProductOwnerProcessed,adlsTargetDirectory,"DataProductOwner",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductOwner"),Some(df_dataProductOwnerProcessed),jobRunGuid, "DataProductOwner","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductOwner"))

        // Relationship is built, write the DataProduct
        dataProduct.writeData(df_dataProductProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProduct"),Some(df_dataProductProcessed),jobRunGuid, "DataProduct","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProduct"))


      }
    } catch
    {
      case e: Exception =>
        println(s"Error In Main DataProduct Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main DataProduct Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(args(2),args(1).concat("/DataProduct"),None,args(4), "DataProduct",s"Error In Main DataProduct Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main DataProduct Spark Application!: ${e.getMessage}")
    }
  }
}
