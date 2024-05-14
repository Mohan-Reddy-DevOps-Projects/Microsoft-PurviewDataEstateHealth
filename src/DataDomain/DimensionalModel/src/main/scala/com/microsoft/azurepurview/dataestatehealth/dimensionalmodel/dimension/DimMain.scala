package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{Maintenance, Reader, Writer}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.{DataFrame, Row, SparkSession}

object DimMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(AdlsTargetDirectory:String,ReProcessingThresholdInMins:Int,AccountId:String,JobRunGuid:String,spark:SparkSession): Unit = {
        try {
          println("In DimMain Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the DimMain Main Spark Application!")

          println(
            s"""Received parameters:
               |Target ADLS Path - $AdlsTargetDirectory
               |Re-processing Threshold (in minutes) - $ReProcessingThresholdInMins
               |AccountId - $AccountId
               |JobRunGuid - $JobRunGuid""".stripMargin)

          // Processing DimDate Delta Table

          val dimDateSchema = new DimDateSchema().dimDateSchema
          val dimDate = new DimDate(spark,logger)
          val df_dimDateProcessed = dimDate.populateDimTimeTable(dimDateSchema)
          val writer = new Writer(logger)
          writer.overWriteData(df_dimDateProcessed,AdlsTargetDirectory,"DimDate",ReProcessingThresholdInMins)
          val VacuumOptimize= new Maintenance(spark,logger)
          VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDate"),Some(df_dimDateProcessed),JobRunGuid, "DimDate","")
          VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDate"))

          // Processing DimBusinessDomain
          val dimBusinessDomainSchema = new DimBusinessDomainSchema().dimBusinessDomainSchema
          val dimBusinessDomain = new DimBusinessDomain(spark,logger)
          val reader = new Reader(spark,logger)
          val df_SourceBusinessDomain: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"BusinessDomain")
          df_SourceBusinessDomain match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimBusinessDomainProcessed =  dimBusinessDomain.processDimBusinessDomain(df,dimBusinessDomainSchema,AdlsTargetDirectory.concat("/DimensionalModel/DimBusinessDomain"))
              dimBusinessDomain.writeData(df_dimBusinessDomainProcessed,AdlsTargetDirectory,"incremental",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimBusinessDomain"),Some(df),JobRunGuid, "DimBusinessDomain","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimBusinessDomain"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame = dimBusinessDomain.processEmptyDimBusinessDomain(dimBusinessDomainSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimBusinessDomain",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimBusinessDomain"),Some(emptyDataFrame),JobRunGuid, "DimBusinessDomain","")
              println("Failed to read BusinessDomain DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDataProduct
          val dimDataProductSchema = new DimDataProductSchema().dimDataProductSchemaSchema
          val dimDataProduct = new DimDataProduct(spark,logger)
          val df_SourceDataProduct: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataProduct")
          df_SourceDataProduct match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimDataProductProcessed = dimDataProduct.processDimDataProduct(df,dimDataProductSchema,AdlsTargetDirectory)
              dimDataProduct.writeData(df_dimDataProductProcessed,AdlsTargetDirectory,"incremental",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataProduct"),Some(df),JobRunGuid, "DimDataProduct","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDataProduct"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame = dimDataProduct.processEmptyDimDataProduct(dimDataProductSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDataProduct",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataProduct"),Some(emptyDataFrame),JobRunGuid, "DimDataProduct","")
              println("Failed to read DataProduct DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }
          
          // Processing DimDataAsset
          val dimDataAssetSchema = new DimDataAssetSchema().dimDataAssetSchema
          val dimDataAsset = new DimDataAsset(spark,logger)
          val df_SourceDataAsset: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataAsset")
          df_SourceDataAsset match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_DimDataAssetProcessed = dimDataAsset.processDimDataAsset(df,dimDataAssetSchema,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAsset"))
              dimDataAsset.writeData(df_DimDataAssetProcessed,AdlsTargetDirectory,"incremental",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAsset"),Some(df),JobRunGuid, "DimDataAsset","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDataAsset"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame = dimDataAsset.processEmptyDimDataAsset(dimDataAssetSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDataAsset",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAsset"),Some(emptyDataFrame),JobRunGuid, "DimDataAsset","")
              println("Failed to read DataAsset DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDataAssetColumn
          val dimDataAssetColumnSchema = new DimDataAssetColumnSchema().dimDataAssetColumnSchema
          val dimDataAssetColumn = new DimDataAssetColumn(spark,logger)
          val df_SourceDataAssetColumn: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataAssetColumn")
          df_SourceDataAssetColumn match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimDataAssetColumnProcessed = dimDataAssetColumn.processDimDataAssetColumn(df,dimDataAssetColumnSchema,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAssetColumn"))
              dimDataAssetColumn.writeData(df_dimDataAssetColumnProcessed,AdlsTargetDirectory,"incremental",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAssetColumn"),Some(df),JobRunGuid, "DimDataAssetColumn","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDataAssetColumn"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame =  dimDataAssetColumn.processEmptyDimDataAssetColumn(dimDataAssetColumnSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDataAssetColumn",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataAssetColumn"),Some(emptyDataFrame),JobRunGuid, "DimDataAssetColumn","")
              println("Failed to read DataAssetColumn DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDQRuleName Delta Table

          val dimDQRuleNameSchema = new DimDQRuleNameSchema().dimDQRuleNameSchema
          val dimDQRuleName = new DimDQRuleName(spark,logger)
          val df_SourceDataQualityRule: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataQualityRule")
          df_SourceDataQualityRule match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimDQRuleNameProcessed = dimDQRuleName.processDimDQRuleName(df,dimDQRuleNameSchema)
              writer.overWriteData(df_dimDQRuleNameProcessed,AdlsTargetDirectory,"DimDQRuleName",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleName"),Some(df),JobRunGuid, "DimDQRuleName","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleName"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame =  dimDQRuleName.processEmptyDimDQRuleName(dimDQRuleNameSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDQRuleName",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleName"),Some(emptyDataFrame),JobRunGuid, "DimDQRuleName","")
              println("Failed to read DataQualityRule DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDQRuleType Delta Table

          val dimDQRuleTypeSchema = new DimDQRuleTypeSchema().dimDQRuleTypeSchema
          val dimDQRuleType = new DimDQRuleType(spark,logger)
          val df_SourceDimDQRuleType: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataQualityRuleType")
          df_SourceDimDQRuleType match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimDQRuleTypeProcessed = dimDQRuleType.processDimDQRuleType(df,dimDQRuleTypeSchema)
              writer.overWriteData(df_dimDQRuleTypeProcessed,AdlsTargetDirectory,"DimDQRuleType",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleType"),Some(df),JobRunGuid, "DimDQRuleType","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleType"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame = dimDQRuleType.processEmptyDimDQRuleType(dimDQRuleTypeSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDQRuleType",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQRuleType"),Some(emptyDataFrame),JobRunGuid, "DimDQRuleType","")
              println("Failed to read DimDQRuleType DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDQScanProfile Delta Table

          val dimDQScanProfileSchema = new DimDQScanProfileSchema().dimDQScanProfileSchema
          val dimDQScanProfile = new DimDQScanProfile(spark,logger)
          val df_SourceDimDQScanProfile: Option[DataFrame] = reader.readAdlsDelta(AdlsTargetDirectory,"DataQualityRule")
          df_SourceDimDQScanProfile match {
            case Some(df) =>
              // DataFrame exists, perform further processing
              val df_dimDQScanProfileProcessed = dimDQScanProfile.processDimDQScanProfile(df,dimDQScanProfileSchema)
              writer.overWriteData(df_dimDQScanProfileProcessed,AdlsTargetDirectory,"DimDQScanProfile",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQScanProfile"),Some(df),JobRunGuid, "DimDQScanProfile","")
              VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDQScanProfile"))
            case None =>
              // DataFrame does not exist, handle the case
              val emptyDataFrame: DataFrame = dimDQScanProfile.processEmptyDimDQScanProfile(dimDQScanProfileSchema)
              writer.overWriteData(emptyDataFrame,AdlsTargetDirectory,"DimDQScanProfile",ReProcessingThresholdInMins)
              VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQScanProfile"),Some(emptyDataFrame),JobRunGuid, "DimDQScanProfile","")
              println("Failed to read DataQualityRule DomainModel DataFrame from Delta table. Either Empty or Doest not exist, Creating Empty Table With 1 NOT-AVAILABLE RECORD, Exiting...!")
          }

          // Processing DimDataHealthControl
          val dimDataHealthControlSchema = new DimDataHealthControlSchema().dimDataHealthControlSchema
          val dimDataHealthControl = new DimDataHealthControl(spark, logger)
          val df_dimDataHealthControlProcessed = dimDataHealthControl.processDimDataHealthControl(dimDataHealthControlSchema)
          writer.overWriteData(df_dimDataHealthControlProcessed,AdlsTargetDirectory,"DimDataHealthControl",ReProcessingThresholdInMins)
          VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDataHealthControl"),Some(df_dimDataHealthControlProcessed),JobRunGuid, "DimDataHealthControl","")
          VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDataHealthControl"))

          // Processing DimDQJobType
          val dimDQJobTypeSchema = new DimDQJobTypeSchema().dimDQJobTypeSchema
          val dimDQJobType = new DimDQJobType(spark,logger)
          val df_dimDQJobTypeProcessed = dimDQJobType.processDimDQJobType(dimDQJobTypeSchema)
          writer.overWriteData(df_dimDQJobTypeProcessed,AdlsTargetDirectory,"DimDQJobType",ReProcessingThresholdInMins)
          VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/DimDQJobType"),Some(df_dimDQJobTypeProcessed),JobRunGuid, "DimDQJobType","")
          VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/DimDQJobType"))

        } catch {
          case e: Exception =>
            println(s"Error In DimMain Spark Application!: ${e.getMessage}")
            logger.error(s"Error In DimMain Spark Application!: ${e.getMessage}")
            val VacuumOptimize = new Maintenance(spark,logger)
            VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory,None,JobRunGuid, "DimMain",s"Error In DimMain Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In DimMain Main Application!: ${e.getMessage}")
        }
    }
  }
