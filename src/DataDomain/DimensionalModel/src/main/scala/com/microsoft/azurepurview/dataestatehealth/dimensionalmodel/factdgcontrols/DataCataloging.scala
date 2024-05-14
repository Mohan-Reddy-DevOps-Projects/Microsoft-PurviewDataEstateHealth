package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.factdgcontrols

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{CreateAssetTempView, FactDependencySoftCheck}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, lit}
import org.apache.spark.sql.types.{DoubleType, IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class DataCataloging (spark: SparkSession, logger:Logger){
  def processFactDataGovernanceScanControlDataCataloging(schema: org.apache.spark.sql.types.StructType,targetAdlsRootPath:String):DataFrame=  {
    try {
      val factDependencySoftCheck = new FactDependencySoftCheck(spark, logger)

      val listOfDomainAssetsFactDepCheck = List(
        "DataProduct"
      )

      val listOfDomainAssets = List(
        "DataProduct"
        , "BusinessDomain"
        , "DataProductStatus"
        , "DataProductBusinessDomainAssignment"
        , "DataAsset"
        , "DataProductAssetAssignment"
        , "DataAssetOwnerAssignment"
        , "DataProductOwner"
        , "GlossaryTermDataProductAssignment"
      )

      val listOfDimensionalAssets = List(
        "DimDataHealthControl"
        , "DimDataProduct"
        , "DimBusinessDomain"
        , "DimDate")

      if (factDependencySoftCheck.checkAssets("FactDataGovernanceScan-DataCataloging",targetAdlsRootPath.concat("/").concat("DomainModel"),listOfDomainAssetsFactDepCheck)
        && factDependencySoftCheck.checkAssets("FactDataGovernanceScan-DataCataloging",targetAdlsRootPath.concat("/").concat("DimensionalModel"),listOfDimensionalAssets))
      {
      val tempViewProcessor = new CreateAssetTempView(spark,logger)
      tempViewProcessor.processAssets(targetAdlsRootPath.concat("/").concat("DomainModel"),listOfDomainAssets)
      tempViewProcessor.processAssets(targetAdlsRootPath.concat("/").concat("DimensionalModel"),listOfDimensionalAssets)

      var dfProcess = spark.sql("""SELECT (SELECT HealthControlId FROM DimDataHealthControl
                                  | WHERE HealthControlDisplayName='Data Cataloging'
                                  | AND HealthControlGroupDisplayName='Discoverability And Understanding') AS HealthControlId
                                  |,DDP.DataProductId AS DataProductId
                                  |,CAST(DATE_FORMAT(CURRENT_TIMESTAMP(), 'yyyyMMdd') AS INT) AS DEHProcessingDateId
                                  |,CURRENT_TIMESTAMP() AS LastProcessedDatetime
                                  |,COALESCE(DBD.BusinessDomainId,(SELECT BusinessDomainId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainId
                                  |,ControlQuery.TotalDataProductAssetCount AS TotalDataProductAssetCount
                                  |,ControlQuery.DataProductCounter AS DataProductCounter
                                  |,ControlQuery.DataProductDataAssetHasOwnerCount AS DataProductDataAssetHasOwnerCount
                                  |,ControlQuery.DataProductOwnerScore AS DataProductOwnerScore
                                  |,ControlQuery.DataProductHasOwner AS DataProductHasOwner
                                  |,ControlQuery.DataProductPublishedCount AS DataProductPublishedCount
                                  |,ControlQuery.DataProductDraftCount AS DataProductDraftCount
                                  |,ControlQuery.DataProductExpiredCount AS DataProductExpiredCount
                                  |,ControlQuery.DataProductPublishedByAuthorizedUserCount AS DataProductPublishedByAuthorizedUserCount
                                  |,ControlQuery.DataProductHasDescription AS DataProductHasDescription
                                  |,ControlQuery.DataProductHasAsset AS DataProductHasAsset
                                  |,ControlQuery.DataProductHasPublishedGlossaryTerm AS DataProductHasPublishedGlossaryTerm
                                  |,ControlQuery.DataProductHasUseCase AS DataProductHasUseCase
                                  |,ControlQuery.DataProductDataCatalogingScore AS DataProductDataCatalogingScore
                                  | FROM (
                                  | SELECT DP.DataProductId
                                  |,BD.BusinessDomainId
                                  |,1 AS DataProductCounter
                                  |,COALESCE(AC.DataProductAssetCount,0) AS TotalDataProductAssetCount
                                  |,COALESCE(DPHAWC.DataProductDataAssetHasOwnerCount,0) AS DataProductDataAssetHasOwnerCount
                                  |,COALESCE(DPO.DataProductHasOwner,0) AS DataProductHasOwner
                                  |,CAST(COALESCE(DPO.DataProductHasOwner,0) AS DECIMAL) / 1 AS DataProductOwnerScore
                                  |, CASE WHEN DPS.DataProductStatusDisplayName = 'Published' THEN 1 ELSE 0 END DataProductPublishedCount
                                  |, CASE WHEN DPS.DataProductStatusDisplayName = 'Draft' THEN 1 ELSE 0 END DataProductDraftCount
                                  |, CASE WHEN DPS.DataProductStatusDisplayName = 'Expired' THEN 1 ELSE 0 END DataProductExpiredCount
                                  |, CASE WHEN DPS.DataProductStatusDisplayName = 'Published' THEN 1 ELSE 0 END DataProductPublishedByAuthorizedUserCount
                                  |, CASE WHEN DP.DataProductDescription IS NOT NULL THEN 1 ELSE 0 END DataProductHasDescription
                                  |, CASE WHEN AC.DataProductHasAsset=1 THEN 1 ELSE 0 END AS DataProductHasAsset
                                  |, CASE WHEN GT.DataProductId IS NOT NULL THEN 1 ELSE 0 END AS DataProductHasPublishedGlossaryTerm
                                  |, CASE WHEN DP.UseCases IS NOT NULL THEN 1 ELSE 0 END DataProductHasUseCase
                                  |, CAST((CASE WHEN DP.DataProductDescription IS NOT NULL THEN 1 ELSE 0 END +
                                  |	 CASE WHEN GT.DataProductId IS NOT NULL THEN 1 ELSE 0 END +
                                  |	 CASE WHEN DP.UseCases IS NOT NULL THEN 1 ELSE 0 END
                                  |	) AS DECIMAL ) / 3 AS DataProductDataCatalogingScore
                                  |FROM DataProduct DP
                                  |INNER JOIN DataProductStatus DPS
                                  |ON DP.DataProductStatusID = DPS.DataProductStatusID
                                  |LEFT JOIN DataProductBusinessDomainAssignment DPBDA
                                  |ON DP.DataProductId = DPBDA.DataProductId
                                  |LEFT JOIN BusinessDomain BD
                                  |ON DPBDA.BusinessDomainId = BD.BusinessDomainId
                                  |
                                  |LEFT JOIN (SELECT DPAA.DataProductId, COUNT(DA.DataAssetId) DataProductAssetCount,1 AS DataProductHasAsset
                                  |			FROM DataAsset DA
                                  |			INNER JOIN DataProductAssetAssignment DPAA
                                  |			ON DA.DataAssetId=DPAA.DataAssetId
                                  |			GROUP BY DPAA.DataProductId) AC
                                  |ON DP.DataProductId = AC.DataProductId
                                  |LEFT JOIN
                                  |(SELECT DPAA.DataProductId,COUNT(DAOA.DataAssetId) DataProductDataAssetHasOwnerCount
                                  |FROM DataProductAssetAssignment DPAA
                                  |INNER JOIN DataAssetOwnerAssignment DAOA
                                  |ON DPAA.DataAssetId = DAOA.DataAssetId
                                  |GROUP BY DPAA.DataProductId) DPHAWC
                                  |ON DP.DataProductId = DPHAWC.DataProductId
                                  |LEFT JOIN
                                  |(SELECT DISTINCT DP.DataProductId,DPS.DataProductStatusDisplayName
                                  | FROM GlossaryTermDataProductAssignment GTDPA
                                  | INNER JOIN DataProduct DP
                                  | ON DP.DataProductId = GTDPA.DataProductId
                                  | INNER JOIN DataProductStatus DPS
                                  | ON DP.DataProductStatusID = DPS.DataProductStatusID AND LOWER(DPS.DataProductStatusDisplayName) = 'published') GT
                                  | ON DP.DataProductId=GT.DataProductId
                                  |LEFT JOIN
                                  |(SELECT DISTINCT DataProductId, 1 AS DataProductHasOwner
                                  |FROM DataProductOwner) DPO
                                  |ON DP.DataProductId = DPO.DataProductId
                                  | WHERE DP.ExpiredFlag=0) ControlQuery
                                  | INNER JOIN
                                  | DimDataProduct DDP ON DDP.DataProductSourceId = ControlQuery.DataProductId
                                  | LEFT JOIN DimBusinessDomain DBD ON ControlQuery.BusinessDomainId = DBD.BusinessDomainSourceId""".stripMargin)

      dfProcess = dfProcess.select(
        col("HealthControlId").cast(IntegerType),
        col("DataProductId").cast(StringType),
        col("DEHProcessingDateId").cast(LongType),
        col("LastProcessedDatetime").cast(TimestampType),
        col("BusinessDomainId").cast(StringType),
        col("TotalDataProductAssetCount").cast(IntegerType),
        col("DataProductCounter").cast(IntegerType),
        lit(null).cast(IntegerType).as("ClassifiedAssetCount"),
        lit(null).cast(DoubleType).as("DataProductClassificationScore"),
        lit(null).cast(IntegerType).as("DataProductIsClassified"),
        lit(null).cast(IntegerType).as("LabeledAssetCount"),
        lit(null).cast(DoubleType).as("DataProductLabelScore"),
        lit(null).cast(IntegerType).as("DataProductIsLabeled"),
        lit(null).cast(DoubleType).as("DataProductOwnerScore"),
        lit(null).cast(IntegerType).as("DataProductHasOwner"),
        lit(null).cast(IntegerType).as("DataProductDataAssetHasOwnerCount"),
        lit(null).cast(DoubleType).as("DataProductObservabilityScore"),
        lit(1.00000).cast(DoubleType).as("DataProductAssetLinkageScore"),
        lit(null).cast(IntegerType).as("DataProductAssetDQEnabledCount"),
        lit(null).cast(IntegerType).as("DataProductAssetDQNotEnabledCount"),
        lit(null).cast(DoubleType).as("DataProductAssetDQEnabledScore"),
        lit(null).cast(DoubleType).as("DataProductCompositeDQScore"),
        lit(null).cast(DoubleType).as("DataProductOKRLinkageScore"),
        lit(null).cast(IntegerType).as("DataProductOKRLinkageCount"),
        lit(null).cast(DoubleType).as("DataProductCriticalDataLinkageScore"),
        lit(null).cast(IntegerType).as("DataProductCriticalDataLinkageCount"),
        lit(null).cast(DoubleType).as("DataProductSelfServiceEnablementScore"),
        lit(null).cast(IntegerType).as("DataProductSelfServiceAccessPolicyCount"),
        lit(null).cast(IntegerType).as("DataProductSelfServiceDurationPolicyCount"),
        lit(null).cast(IntegerType).as("DataProductSelfServiceSLAPolicyCount"),
        lit(null).cast(DoubleType).as("DataProductComplianceDataTermsOfUsePolicyScore"),
        lit(null).cast(IntegerType).as("DataProductComplianceDataTermsOfUsePolicyCount"),
        col("DataProductPublishedCount").cast(IntegerType),
        col("DataProductDraftCount").cast(IntegerType),
        col("DataProductExpiredCount").cast(IntegerType),
        col("DataProductHasDescription").cast(IntegerType),
        lit(null).cast(IntegerType).as("DataProductHasAsset"),
        col("DataProductHasPublishedGlossaryTerm").cast(IntegerType),
        lit(null).cast(IntegerType).as("DataProductHasGlossaryTerm"),
        col("DataProductHasUseCase").cast(IntegerType),
        col("DataProductDataCatalogingScore").cast(DoubleType),
        col("DataProductPublishedByAuthorizedUserCount").cast(IntegerType),
        lit(null).cast(IntegerType).as("DataProductAssetMDQEnabledCount"),
        lit(null).cast(IntegerType).as("DataProductAssetMDQNotEnabledCount"),
        lit(null).cast(DoubleType).as("DataProductAssetMDQEnabledScore"),
        lit(null).cast(IntegerType).as("DataProductCertifiedCount"),
        lit(null).cast(IntegerType).as("DataProductAccessGrantCount"),
        lit(null).cast(IntegerType).as("DataProductInSLAAccessGrantCount"),
        lit(null).cast(DoubleType).as("DataProductAccessGrantScore"),
        lit(null).cast(IntegerType).as("DataProductDescHasLenGr100Char"),
        lit(null).cast(IntegerType).as("DataProductAssoBusinessDomainDescHasLenGr100Char"),
        lit(null).cast(IntegerType).as("DataProductAssoAllTremDescHasLenGr25Char"),
        lit(null).cast(DoubleType).as( "DataProductUsabilityScore"),
        lit(null).cast(IntegerType).as("DataProductHasParentBusinessDomain"),
        lit(null).cast(IntegerType).as("DataProductHasAtLeast1Asset"),
        lit(null).cast(IntegerType).as("DataProductHasAssoPubTerm"),
        lit(null).cast(DoubleType).as("LinkedAssetsScore"),
        lit(null).cast(IntegerType).as("DataProductAllAssoAssetsHasOwners"),
        lit(null).cast(DoubleType).as("OwnershipScore")
      )
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    else
    {
      spark.emptyDataFrame
    }
    } catch {
      case e: Exception =>
        println(s"Error Processing FactDataGovernanceScanControlDataCataloging Control: ${e.getMessage}")
        logger.error(s"Error Processing FactDataGovernanceScanControlDataCataloging Control: ${e.getMessage}")
        throw e
    }
  }
}
