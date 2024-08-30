package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{ColdStartSoftCheck, Maintenance, Reader, Writer}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.{Level, Logger}
object DataAssetMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String],spark:SparkSession,ReProcessingThresholdInMins:Int): Unit = {
    try{
      println("In DataAsset Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the DataAsset Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark,logger)
      if (args.length >= 5 && coldStartSoftCheck.validateCheckIn(args(0),"dataasset")) {
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
        val dataAssetContractSchema = new DataAssetContractSchema().dataAssetContractSchema
        val dataAssetSchema = new DataAssetSchema().dataAssetSchema
        val reader = new Reader(spark, logger)
        val df_Asset = reader.readCosmosData(dataAssetContractSchema,CosmosDBLinkedServiceName,accountId,"dataasset","DataCatalog","DataAsset")
        val dataAsset = new DataAsset(spark,logger)
        //Process DataAssetType
        val dataAssetTypeSchema = new DataAssetTypeSchema().dataAssetTypeSchema
        val dataAssetType = new DataAssetType(spark,logger)
        val df_dataAssetTypeProcessed = dataAssetType.processDataAssetType(df_Asset,dataAssetTypeSchema)
        val writer = new Writer(logger)
        writer.overWriteData(df_dataAssetTypeProcessed,adlsTargetDirectory,"DataAssetType",ReProcessingThresholdInMins)
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetType"),Some(df_dataAssetTypeProcessed),jobRunGuid, "DataAssetType","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetType"))

        //Process DataAssetOwner
        val dataAssetOwnerSchema = new DataAssetOwnerSchema().dataAssetOwnerSchema
        val dataAssetOwner = new DataAssetOwner(spark,logger)
        val df_dataAssetOwnerProcessed = dataAssetOwner.processDataAssetOwner(df_Asset,dataAssetOwnerSchema)
        writer.overWriteData(df_dataAssetOwnerProcessed,adlsTargetDirectory,"DataAssetOwner",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetOwner"),Some(df_dataAssetOwnerProcessed),jobRunGuid, "DataAssetOwner","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetOwner"))

        //Process Classification
        val classificationSchema = new ClassificationSchema().classificationSchema
        val classification = new Classification(spark,logger)
        val df_classificationProcessed = classification.processClassification(df_Asset,classificationSchema)
        writer.overWriteData(df_classificationProcessed,adlsTargetDirectory,"Classification",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/Classification"),Some(df_classificationProcessed),jobRunGuid, "Classification","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/Classification"))

        //Data Asset Column
        val dataAssetColumnSchema = new DataAssetColumnSchema().dataAssetColumnSchema
        val dataAssetColumn = new DataAssetColumn(spark,logger)
        val df_dataAssetColumn = df_Asset
        val df_dataAssetColumnProcessed = dataAssetColumn.processDataAssetColumn(df_dataAssetColumn,dataAssetColumnSchema,adlsTargetDirectory)

        //// Process DataAssetTypeDataType
        val dataAssetTypeDataTypeSchema = new DataAssetTypeDataTypeSchema().dataAssetTypeDataTypeSchema
        val dataAssetTypeDataType = new DataAssetTypeDataType(spark,logger)
        val df_dataAssetTypeDataTypeProcessed = dataAssetTypeDataType.processDataAssetTypeDataType(df_dataAssetColumnProcessed,dataAssetTypeDataTypeSchema)
        writer.overWriteData(df_dataAssetTypeDataTypeProcessed,adlsTargetDirectory,"DataAssetTypeDataType",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetTypeDataType"),Some(df_dataAssetTypeDataTypeProcessed),jobRunGuid, "DataAssetTypeDataType","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetTypeDataType"))

        ////Process DataAssetColumnClassificationAssignment
        val dataAssetColumnClassificationAssignmentSchema = new DataAssetColumnClassificationAssignmentSchema().dataAssetColumnClassificationAssignmentSchema
        val dataAssetColumnClassificationAssignment = new DataAssetColumnClassificationAssignment(spark,logger)
        val df_dataAssetColumnClassificationAssignmentProcessed = dataAssetColumnClassificationAssignment.processDataAssetColumnClassificationAssignment(df_dataAssetColumnProcessed,dataAssetColumnClassificationAssignmentSchema)
        writer.overWriteData(df_dataAssetColumnClassificationAssignmentProcessed,adlsTargetDirectory,"DataAssetColumnClassificationAssignment",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetColumnClassificationAssignment"),Some(df_dataAssetColumnClassificationAssignmentProcessed),jobRunGuid, "DataAssetColumnClassificationAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetColumnClassificationAssignment"))

        //Now Complete the Remaining Processing for DataAssetColumn
        dataAssetColumn.writeData(df_dataAssetColumnProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetColumn"),Some(df_dataAssetColumnProcessed),jobRunGuid, "DataAssetColumn","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetColumn"))

        // Process DataAssetOwnerAssignment
        val dataAssetOwnerAssignmentSchema = new DataAssetOwnerAssignmentSchema().dataAssetOwnerAssignmentSchema
        val dataAssetOwnerAssignment = new DataAssetOwnerAssignment(spark,logger)
        val df_dataAssetOwnerAssignmentProcessed = dataAssetOwnerAssignment.processDataAssetOwnerAssignment(df_Asset,dataAssetOwnerAssignmentSchema)
        writer.overWriteData(df_dataAssetOwnerAssignmentProcessed,adlsTargetDirectory,"DataAssetOwnerAssignment",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetOwnerAssignment"),Some(df_dataAssetOwnerAssignmentProcessed),jobRunGuid, "DataAssetOwnerAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetOwnerAssignment"))

        // Process DataProductAssetAssignment
        val dataProductAssetAssignmentSchema = new DataProductAssetAssignmentSchema().dataProductAssetAssignmentSchema
        val dataProductAssetAssignment = new DataProductAssetAssignment(spark,logger)
        val df_dataProductAssetAssignmentProcessed = dataProductAssetAssignment.processDataProductAssetAssignment(dataProductAssetAssignmentSchema,adlsTargetDirectory)
        writer.overWriteData(df_dataProductAssetAssignmentProcessed,adlsTargetDirectory,"DataProductAssetAssignment",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataProductAssetAssignment"),Some(df_dataProductAssetAssignmentProcessed),jobRunGuid, "DataProductAssetAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductAssetAssignment"))

        // Finally Process DataAsset
        val df_dataAssetProcessed = dataAsset.processDataAsset(df_Asset,dataAssetSchema)
        dataAsset.writeData(df_dataAssetProcessed,adlsTargetDirectory,refreshType,ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAsset"),Some(df_dataAssetProcessed),jobRunGuid, "DataAsset","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAsset"))

        // Read Delta tables
        println("Applied Global Asset Migration Changes.")
        val dataAssetDomainAssignment = new DataAssetDomainAssignment(spark,logger)
        val dataAssetDomainAssignmentSchema = new DataAssetDomainAssignmentSchema().dataAssetDomainAssignmentSchema
        val dfDataProductAssetAssignment: Option[DataFrame] = reader.readAdlsDelta(DeltaPath = adlsTargetDirectory, Entity = "/DataProductAssetAssignment")
        val dfDataProductBusinessDomainAssignment: Option[DataFrame] = reader.readAdlsDelta(DeltaPath = adlsTargetDirectory, Entity = "/DataProductBusinessDomainAssignment")
        val dfDataAssetDomainAssignment = dataAssetDomainAssignment.extractDataAssetDomainMapping(
          dfDataProductAssetAssignment = dfDataProductAssetAssignment,
          dfDataProductBusinessDomainAssignment = dfDataProductBusinessDomainAssignment,
          schema = dataAssetDomainAssignmentSchema)
        writer.overWriteData(dfDataAssetDomainAssignment,adlsTargetDirectory,"DataAssetDomainAssignment",ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(accountId,adlsTargetDirectory.concat("/DataAssetDomainAssignment"),Some(dfDataAssetDomainAssignment),jobRunGuid, "DataAssetDomainAssignment","")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetDomainAssignment"))
      }
    } catch
    {
      case e: Exception =>
        println(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(args(2),args(1).concat("/DataAsset"),None,args(4), "DataAsset",s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
    }
  }
}
