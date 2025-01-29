package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.{DataWriter, Maintenance, Reader}
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.ColdStartSoftCheck
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number}
import org.apache.spark.sql.{DataFrame, SparkSession}

object DataAssetMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String], spark: SparkSession, ReProcessingThresholdInMins: Int): Unit = {
    try {
      println("In DataAsset Spark Application!")

      logger.setLevel(Level.INFO)
      logger.info("Started the DataAsset Main Application!")
      val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
      if (args.length >= 4 && coldStartSoftCheck.validateCheckIn("dataasset")) {
        val adlsTargetDirectory = args(0)
        val accountId = args(1)
        val refreshType = args(2)
        val jobRunGuid = args(3)
        println(
          s"""Received parameters: Target ADLS Path - $adlsTargetDirectory
        , AccountId - $accountId
        , Processing Type - $refreshType
        , JobRunGuid - $jobRunGuid""")
        val dataAssetContractSchema = new DataAssetContractSchema().dataAssetContractSchema
        val dataAssetSchema = new DataAssetSchema().dataAssetSchema
        val reader = new Reader(spark, logger)
        val df_Asset = reader.readCosmosData(dataAssetContractSchema,"", accountId, "dataasset", "DataCatalog", "DataAsset")
        val dataAsset = new DataAsset(spark, logger)
        //Process DataAssetType
        val dataAssetTypeSchema = new DataAssetTypeSchema().dataAssetTypeSchema
        val dataAssetType = new DataAssetType(spark, logger)
        val df_dataAssetTypeProcessed = dataAssetType.processDataAssetType(df_Asset, dataAssetTypeSchema)
        val dataWriter = new DataWriter(spark)
        dataWriter.writeData(df_dataAssetTypeProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetType")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetType"), Some(df_dataAssetTypeProcessed), jobRunGuid, "DataAssetType", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetType"))

        //Process DataAssetOwner
        val dataAssetOwnerSchema = new DataAssetOwnerSchema().dataAssetOwnerSchema
        val dataAssetOwner = new DataAssetOwner(spark, logger)
        val df_dataAssetOwnerProcessed = dataAssetOwner.processDataAssetOwner(df_Asset, dataAssetOwnerSchema)
        dataWriter.writeData(df_dataAssetOwnerProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetOwner")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetOwner"), Some(df_dataAssetOwnerProcessed), jobRunGuid, "DataAssetOwner", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetOwner"))

        //Process Classification
        val classificationSchema = new ClassificationSchema().classificationSchema
        val classification = new Classification(spark, logger)
        val df_classificationProcessed = classification.processClassification(df_Asset, classificationSchema)
        dataWriter.writeData(df_classificationProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "Classification")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/Classification"), Some(df_classificationProcessed), jobRunGuid, "Classification", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/Classification"))

        //Data Asset Column
        val dataAssetColumnSchema = new DataAssetColumnSchema().dataAssetColumnSchema
        val dataAssetColumn = new DataAssetColumn(spark, logger)
        val df_dataAssetColumn = df_Asset
        val df_dataAssetColumnProcessed = dataAssetColumn.processDataAssetColumn(df_dataAssetColumn, dataAssetColumnSchema, adlsTargetDirectory)

        //// Process DataAssetTypeDataType
        val dataAssetTypeDataTypeSchema = new DataAssetTypeDataTypeSchema().dataAssetTypeDataTypeSchema
        val dataAssetTypeDataType = new DataAssetTypeDataType(spark, logger)
        val df_dataAssetTypeDataTypeProcessed = dataAssetTypeDataType.processDataAssetTypeDataType(df_dataAssetColumnProcessed, dataAssetTypeDataTypeSchema)
        dataWriter.writeData(df_dataAssetTypeDataTypeProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetTypeDataType")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetTypeDataType"), Some(df_dataAssetTypeDataTypeProcessed), jobRunGuid, "DataAssetTypeDataType", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetTypeDataType"))

        ////Process DataAssetColumnClassificationAssignment
        val dataAssetColumnClassificationAssignmentSchema = new DataAssetColumnClassificationAssignmentSchema().dataAssetColumnClassificationAssignmentSchema
        val dataAssetColumnClassificationAssignment = new DataAssetColumnClassificationAssignment(spark, logger)
        val df_dataAssetColumnClassificationAssignmentProcessed = dataAssetColumnClassificationAssignment.processDataAssetColumnClassificationAssignment(df_dataAssetColumnProcessed, dataAssetColumnClassificationAssignmentSchema)
        dataWriter.writeData(df_dataAssetColumnClassificationAssignmentProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetColumnClassificationAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetColumnClassificationAssignment"), Some(df_dataAssetColumnClassificationAssignmentProcessed), jobRunGuid, "DataAssetColumnClassificationAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetColumnClassificationAssignment"))
        val df_dataAssetColumnFiltered = df_dataAssetColumnProcessed.drop("ClassificationId")
          .drop("ColumnDataType")
          .drop("ColumnClassification")
          .distinct()

        val windowSpec = Window.partitionBy("DataAssetId", "ColumnId").orderBy(col("ModifiedDateTime").desc)
        val finalDfDataAssetColumn = df_dataAssetColumnFiltered.withColumn("row_number", row_number().over(windowSpec)).filter(col("row_number") === 1)
          .drop("row_number")
        dataWriter.writeData(finalDfDataAssetColumn, adlsTargetDirectory, ReProcessingThresholdInMins
          , "DataAssetColumn", Seq("DataAssetId", "ColumnId"), refreshType)
        //Now Complete the Remaining Processing for DataAssetColumn
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetColumn"), Some(finalDfDataAssetColumn), jobRunGuid, "DataAssetColumn", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetColumn"))

        // Process DataAssetOwnerAssignment
        val dataAssetOwnerAssignmentSchema = new DataAssetOwnerAssignmentSchema().dataAssetOwnerAssignmentSchema
        val dataAssetOwnerAssignment = new DataAssetOwnerAssignment(spark, logger)
        val df_dataAssetOwnerAssignmentProcessed = dataAssetOwnerAssignment.processDataAssetOwnerAssignment(df_Asset, dataAssetOwnerAssignmentSchema)
        dataWriter.writeData(df_dataAssetOwnerAssignmentProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetOwnerAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetOwnerAssignment"), Some(df_dataAssetOwnerAssignmentProcessed), jobRunGuid, "DataAssetOwnerAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetOwnerAssignment"))

        // Process DataProductAssetAssignment
        val dataProductAssetAssignmentSchema = new DataProductAssetAssignmentSchema().dataProductAssetAssignmentSchema
        val dataProductAssetAssignment = new DataProductAssetAssignment(spark, logger)
        val df_dataProductAssetAssignmentProcessed = dataProductAssetAssignment.processDataProductAssetAssignment(dataProductAssetAssignmentSchema, adlsTargetDirectory)
        dataWriter.writeData(df_dataProductAssetAssignmentProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataProductAssetAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataProductAssetAssignment"), Some(df_dataProductAssetAssignmentProcessed), jobRunGuid, "DataProductAssetAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataProductAssetAssignment"))

        // Finally Process DataAsset
        val df_dataAssetProcessed = dataAsset.processDataAsset(df_Asset, dataAssetSchema)
        val finalDataAssetProcessed = df_dataAssetProcessed.drop("BusinessDomainId").distinct()
        dataWriter.writeData(finalDataAssetProcessed, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAsset", Seq("DataAssetId"), refreshType)
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAsset"), Some(finalDataAssetProcessed), jobRunGuid, "DataAsset", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAsset"))

        // Read Delta tables
        println("Applied Global Asset Migration Changes.")
        val dataAssetDomainAssignment = new DataAssetDomainAssignment(spark, logger)
        val dataAssetDomainAssignmentSchema = new DataAssetDomainAssignmentSchema().dataAssetDomainAssignmentSchema
        val dfDataProductAssetAssignment: Option[DataFrame] = reader.readAdlsDelta(DeltaPath = adlsTargetDirectory, Entity = "/DataProductAssetAssignment")
        val dfDataProductBusinessDomainAssignment: Option[DataFrame] = reader.readAdlsDelta(DeltaPath = adlsTargetDirectory, Entity = "/DataProductBusinessDomainAssignment")
        val dfDataAssetDomainAssignment = dataAssetDomainAssignment.extractDataAssetDomainMapping(
          dfDataProductAssetAssignment = dfDataProductAssetAssignment,
          dfDataProductBusinessDomainAssignment = dfDataProductBusinessDomainAssignment,
          schema = dataAssetDomainAssignmentSchema)
        dataWriter.writeData(dfDataAssetDomainAssignment, adlsTargetDirectory
          , ReProcessingThresholdInMins, "DataAssetDomainAssignment")
        VacuumOptimize.checkpointSentinel(accountId, adlsTargetDirectory.concat("/DataAssetDomainAssignment"), Some(dfDataAssetDomainAssignment), jobRunGuid, "DataAssetDomainAssignment", "")
        VacuumOptimize.processDeltaTable(adlsTargetDirectory.concat("/DataAssetDomainAssignment"))
      }
    } catch {
      case e: Exception =>
        println(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        logger.error(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark, logger)
        VacuumOptimize.checkpointSentinel(args(2), args(1).concat("/DataAsset"), None, args(4), "DataAsset", s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In Main DataAsset Spark Application!: ${e.getMessage}")
    }
  }
}
