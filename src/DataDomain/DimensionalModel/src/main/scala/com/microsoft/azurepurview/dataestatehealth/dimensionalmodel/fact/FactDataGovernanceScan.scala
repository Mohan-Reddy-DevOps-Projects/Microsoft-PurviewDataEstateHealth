package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.factdgcontrols.{ClassificationAndLabeling, CompliantDataUse, DGControlsConfigIni, DataCataloging, DataEstateHealthMonitoringAlertingAndInsightsDataEstateHealthObservability, DataProductCertification, DataProductOwnershipTrustedData, DataProductUsabilityMetadataQualityManagement, DataProductsConnectionDiscoverabilityAndUnderstanding, DataQualityMeasurement, DiscoverabilityAndUnderstandingMeasurement, LinkedAssetsMetadataQualityManagement, OwnershipMetadataQualityManagement, SelfServeAccessEnablement}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{current_timestamp, expr}
import org.apache.spark.sql.{DataFrame, SparkSession}

class FactDataGovernanceScan (spark: SparkSession, logger:Logger) {
  def processFactDataGovernanceScan(schema: org.apache.spark.sql.types.StructType, targetAdlsRootPath: String): DataFrame = {
    try {
      var dfProcess = spark.emptyDataFrame
      val dgControlsConfigIni = new DGControlsConfigIni()

      val processFlag = dgControlsConfigIni.getProcessFlag("Classification and Labeling", "Estate Curation")
      val processFlagDQM = dgControlsConfigIni.getProcessFlag("Data Quality Measurement", "Trusted Data")
      val processFlagDC = dgControlsConfigIni.getProcessFlag("Data Cataloging", "Discoverability And Understanding")
      val processFlagSSAE = dgControlsConfigIni.getProcessFlag("Self-serve Access Enablement", "Access And Use")
      val processFlagCDU = dgControlsConfigIni.getProcessFlag("Compliant Data Use", "Access And Use")
      val processFlagDUM = dgControlsConfigIni.getProcessFlag("Discoverability and Understanding Measurement", "Data Discoverability and Understanding (MDQ)")
      val processFlagDPC = dgControlsConfigIni.getProcessFlag("Data Product Certification", "Trusted Data")
      val processFlagDPOTD = dgControlsConfigIni.getProcessFlag("Data Product Ownership", "Trusted Data")
      val processFlagDPCDU = dgControlsConfigIni.getProcessFlag("Data Products Connection", "Discoverability And Understanding")
      val processFlagDEHMAIDEHO = dgControlsConfigIni.getProcessFlag("Data estate health monitoring, alerting, and insights", "Estate Observability")
      val processFlagUMDM = dgControlsConfigIni.getProcessFlag("Usability", "Metadata Quality Management")
      val processFlagLAMDM = dgControlsConfigIni.getProcessFlag("Linked Assets", "Metadata Quality Management")
      val processFlagOMDM = dgControlsConfigIni.getProcessFlag("Ownership", "Metadata Quality Management")

      processFlag match {
        case Some(1) => {
          println("Processing  DG Control - \"Classification and Labeling\" For Control Group \"Estate Curation\"!")
          val classificationAndLabeling = new ClassificationAndLabeling(spark, logger)
          val dfCL = classificationAndLabeling.processFactDataGovernanceScanControlClassificationAndLabeling(schema, targetAdlsRootPath)
          dfProcess =  if (!dfCL.isEmpty) dfCL else dfProcess
        }
        case None => println("Can't Process DG Control - \"Classification and Labeling\" For Control Group \"Estate Curation\"!")
      }
      processFlagDQM match {
        case Some(1) => {
          println("Processing  DG Control - \"Data Quality Measurement\" For Control Group \"Trusted Data\"!")
          val dataQualityMeasurement = new DataQualityMeasurement(spark, logger)
          val dfDQM = dataQualityMeasurement.processFactDataGovernanceScanControlDataQualityMeasurement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDQM.isEmpty) dfDQM else if (!dfProcess.isEmpty && !dfDQM.isEmpty) dfProcess.union(dfDQM) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data Quality Measurement\" For Control Group \"Trusted Data\"!")
      }
      processFlagDC match {
        case Some(1) => {
          println("Processing  DG Control - \"Data Cataloging\" For Control Group \"Discoverability And Understanding\"!")
          val dataCataloging = new DataCataloging(spark, logger)
          val dfDC = dataCataloging.processFactDataGovernanceScanControlDataCataloging(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDC.isEmpty) dfDC else if (!dfProcess.isEmpty && !dfDC.isEmpty) dfProcess.union(dfDC) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data Cataloging\" For Control Group \"Discoverability And Understanding\"!")
      }

      processFlagSSAE match {
        case Some(1) => {
          println("Processing  DG Control - \"Self-serve Access Enablement\" For Control Group \"Access And Use\"!")
          val selfServeAccessEnablement = new SelfServeAccessEnablement(spark, logger)
          val dfSSAE = selfServeAccessEnablement.processFactDataGovernanceScanControlSelfServeAccessEnablement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfSSAE.isEmpty) dfSSAE else if (!dfProcess.isEmpty && !dfSSAE.isEmpty) dfProcess.union(dfSSAE) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Self-serve Access Enablement\" For Control Group \"Access And Use\"!")
      }

      processFlagCDU match {
        case Some(1) => {
          println("Processing  DG Control - \"Compliant Data Use\" For Control Group \"Access And Use\"!")
          val compliantDataUse = new CompliantDataUse(spark, logger)
          val dfCDU = compliantDataUse.processFactDataGovernanceScanControlCompliantDataUse(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfCDU.isEmpty) dfCDU else if (!dfProcess.isEmpty && !dfCDU.isEmpty) dfProcess.union(dfCDU) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Compliant Data Use\" For Control Group \"Access And Use\"!")
      }
/* COMMENTING THE CODE FOR OUT-OF-SCOPE FOR PUBLIC PREVIEW
      processFlagDUM match {
        case Some(1) => {
          println("Processing  DG Control - \"Discoverability and Understanding Measurement\" For Control Group \"Data Discoverability and Understanding (MDQ)\"!")
          val discoverabilityAndUnderstandingMeasurement = new DiscoverabilityAndUnderstandingMeasurement(spark, logger)
          val dfDUM = discoverabilityAndUnderstandingMeasurement.processFactDataGovernanceScanControlDiscoverabilityAndUnderstandingMeasurement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDUM.isEmpty) dfDUM else if (!dfProcess.isEmpty && !dfDUM.isEmpty) dfProcess.union(dfDUM) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Discoverability and Understanding Measurement\" For Control Group \"Data Discoverability and Understanding (MDQ)\"!")
      }
*/
      processFlagDPC match {
        case Some(1) => {
          println("Processing  DG Control - \"Data Product Certification\" For Control Group \"Trusted Data\"!")
          val dataProductCertification = new DataProductCertification(spark, logger)
          val dfDPC = dataProductCertification.processFactDataGovernanceScanControlDataProductCertification(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDPC.isEmpty) dfDPC else if (!dfProcess.isEmpty && !dfDPC.isEmpty) dfProcess.union(dfDPC) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data Product Certification\" For Control Group \"Trusted Data\"!")
      }

      processFlagDPOTD match {
        case Some(1) => {
          println("Processing  DG Control - \"Data Product Ownership\" For Control Group \"Trusted Data\"!")
          val dataProductOwnershipTrustedData = new DataProductOwnershipTrustedData(spark, logger)
          val dfDPOTD = dataProductOwnershipTrustedData.processFactDataGovernanceScanControlDataProductOwnershipTrustedData(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDPOTD.isEmpty) dfDPOTD else if (!dfProcess.isEmpty && !dfDPOTD.isEmpty) dfProcess.union(dfDPOTD) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data Product Ownership\" For Control Group \"Trusted Data\"!")
      }

      processFlagDPCDU match {
        case Some(1) => {
          println("Processing  DG Control - \"Data Products Connection\" For Control Group \"Discoverability And Understanding\"!")
          val dataProductsConnectionDiscoverabilityAndUnderstanding = new DataProductsConnectionDiscoverabilityAndUnderstanding(spark, logger)
          val dfDPCDU = dataProductsConnectionDiscoverabilityAndUnderstanding.processFactDataGovernanceScanControlDataProductsConnectionDiscoverabilityAndUnderstanding(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDPCDU.isEmpty) dfDPCDU else if (!dfProcess.isEmpty && !dfDPCDU.isEmpty) dfProcess.union(dfDPCDU) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data Products Connection\" For Control Group \"Discoverability And Understanding\"!")
      }

      processFlagDEHMAIDEHO match {
        case Some(1) => {
          println("Processing  DG Control - \"Data estate health monitoring, alerting, and insights\" For Control Group \"Estate Observability\"!")
          val dataEstateHealthMonitoringAlertingAndInsightsDataEstateHealthObservability = new DataEstateHealthMonitoringAlertingAndInsightsDataEstateHealthObservability(spark, logger)
          val dfDEHMAIDEHO = dataEstateHealthMonitoringAlertingAndInsightsDataEstateHealthObservability.processFactDataGovernanceScanControlDataEstateHealthMonitoringAlertingAndInsightsDataEstateHealthObservability(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfDEHMAIDEHO.isEmpty) dfDEHMAIDEHO else if (!dfProcess.isEmpty && !dfDEHMAIDEHO.isEmpty) dfProcess.union(dfDEHMAIDEHO) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Data estate health monitoring, alerting, and insights\" For Control Group \"Estate Observability\"!")
      }

      processFlagUMDM match {
        case Some(1) => {
          println("Processing  DG Control - \"Usability\" For Control Group \"Metadata Quality Management\"!")
          val dataProductUsabilityMetadataQualityManagement = new DataProductUsabilityMetadataQualityManagement(spark, logger)
          val dfUMDM = dataProductUsabilityMetadataQualityManagement.processFactDataGovernanceScanControlDataProductUsabilityMetadataQualityManagement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfUMDM.isEmpty) dfUMDM else if (!dfProcess.isEmpty && !dfUMDM.isEmpty) dfProcess.union(dfUMDM) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Usability\" For Control Group \"Metadata Quality Management\"!")
      }
      processFlagLAMDM match {
        case Some(1) => {
          println("Processing  DG Control - \"Linked Assets\" For Control Group \"Metadata Quality Management\"!")
          val linkedAssetsMetadataQualityManagement = new LinkedAssetsMetadataQualityManagement(spark, logger)
          val dfLAMDM = linkedAssetsMetadataQualityManagement.processFactDataGovernanceScanControlLinkedAssetsMetadataQualityManagement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfLAMDM.isEmpty) dfLAMDM else if (!dfProcess.isEmpty && !dfLAMDM.isEmpty) dfProcess.union(dfLAMDM) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Linked Assets\" For Control Group \"Metadata Quality Management\"!")
      }

      /*
      processFlagOMDM match {
        case Some(1) => {
          println("Processing  DG Control - \"Ownership\" For Control Group \"Metadata Quality Management\"!")
          val ownershipMetadataQualityManagement = new OwnershipMetadataQualityManagement(spark, logger)
          val dfOMDM = ownershipMetadataQualityManagement.processFactDataGovernanceScanControlOwnershipMetadataQualityManagement(schema, targetAdlsRootPath)
          dfProcess = if (dfProcess.isEmpty && !dfOMDM.isEmpty) dfOMDM else if (!dfProcess.isEmpty && !dfOMDM.isEmpty) dfProcess.union(dfOMDM) else dfProcess
        }
        case None => println("Can't Process DG Control - \"Ownership\" For Control Group \"Metadata Quality Management\"!")
      }*/

      //Stamp all timestamps 1 single ID for the entire Snapshot.
      if (!dfProcess.isEmpty){
        dfProcess = dfProcess
          .withColumn("DEHProcessingDateId", expr("CAST(DATE_FORMAT(CURRENT_TIMESTAMP(), 'yyyyMMdd') AS LONG)"))
          .withColumn("LastProcessedDatetime", current_timestamp())

        /*dfProcess = dfProcess
          .withColumn("DEHProcessingDateId", expr("CAST(DATE_FORMAT(date_sub(current_timestamp(), 2), 'yyyyMMdd') AS LONG)"))
          .withColumn("LastProcessedDatetime", expr("CAST(date_sub(current_timestamp(), 2) AS TIMESTAMP)"))
         */


      }
      dfProcess
    } catch {
      case e: Exception =>
        println(s"Error Processing FactDataGovernanceScan Fact: ${e.getMessage}")
        logger.error(s"Error Processing FactDataGovernanceScan Fact: ${e.getMessage}")
        throw e
    }
  }
}
