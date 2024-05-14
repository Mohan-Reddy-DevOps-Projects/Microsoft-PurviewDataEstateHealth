package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.factdgcontrols

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{CreateAssetTempView, FactDependencySoftCheck}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, lit}
import org.apache.spark.sql.types.{DoubleType, IntegerType, LongType, StringType, TimestampType}

class DataProductUsabilityMetadataQualityManagement (spark: SparkSession, logger:Logger){
  def processFactDataGovernanceScanControlDataProductUsabilityMetadataQualityManagement(schema: org.apache.spark.sql.types.StructType,targetAdlsRootPath:String):DataFrame=  {
    try {

      val factDependencySoftCheck = new FactDependencySoftCheck(spark, logger)

      val listOfDomainAssetsFactDepCheck = List(
        "DataProduct"
      )

      val listOfDomainAssets = List(
        "DataProduct"
        , "BusinessDomain"
        , "DataProductBusinessDomainAssignment"
        , "DataAsset"
        , "DataProductAssetAssignment"
        , "DataAssetOwnerAssignment"
        , "DataProductOwner"
        , "DataAssetColumnClassificationAssignment"
        , "GlossaryTermDataProductAssignment"
        , "GlossaryTerm"
      )

      val listOfDimensionalAssets = List(
        "DimDataHealthControl"
        , "DimDataProduct"
        , "DimBusinessDomain"
        , "DimDate")

      if (factDependencySoftCheck.checkAssets("FactDataGovernanceScan-DataProductUsabilityMetadataQualityManagement",targetAdlsRootPath.concat("/").concat("DomainModel"),listOfDomainAssetsFactDepCheck)
        && factDependencySoftCheck.checkAssets("FactDataGovernanceScan-DataProductUsabilityMetadataQualityManagement",targetAdlsRootPath.concat("/").concat("DimensionalModel"),listOfDimensionalAssets))
      {
        val tempViewProcessor = new CreateAssetTempView(spark, logger)
        tempViewProcessor.processAssets(targetAdlsRootPath.concat("/").concat("DomainModel"), listOfDomainAssets)
        tempViewProcessor.processAssets(targetAdlsRootPath.concat("/").concat("DimensionalModel"), listOfDimensionalAssets)

        var dfProcess = spark.sql(
          """SELECT (SELECT HealthControlId FROM DimDataHealthControl
            | WHERE HealthControlDisplayName='Usability'
            | AND HealthControlGroupDisplayName='Metadata Quality Management') AS HealthControlId
            |,DDP.DataProductId AS DataProductId
            |,CAST(DATE_FORMAT(CURRENT_TIMESTAMP(), 'yyyyMMdd') AS INT) AS DEHProcessingDateId
            |,CURRENT_TIMESTAMP() AS LastProcessedDatetime
            |,COALESCE(DBD.BusinessDomainId,(SELECT BusinessDomainId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainId
            |,ControlQuery.DataProductAssetCount AS TotalDataProductAssetCount
            |,ControlQuery.DataProductCounter AS DataProductCounter
            |,ControlQuery.ClassifiedAssetCount AS ClassifiedAssetCount
            |,ControlQuery.DataProductClassificationScore AS DataProductClassificationScore
            |,ControlQuery.DataProductIsClassified AS DataProductIsClassified
            |,ControlQuery.DataProductDataAssetHasOwnerCount AS DataProductDataAssetHasOwnerCount
            |,ControlQuery.DataProductHasOwner AS DataProductHasOwner
            |,ControlQuery.DataProductOwnerScore AS DataProductOwnerScore
            |,ControlQuery.DataProductDescHasLenGr100Char
            |,ControlQuery.DataProductAssoBusinessDomainDescHasLenGr100Char
            |,ControlQuery.DataProductAssoAllTremDescHasLenGr25Char
            |,ControlQuery.DataProductUsabilityScore
            | FROM (
            | SELECT DP.DataProductId
            |,BD.BusinessDomainId
            |,COALESCE(AC.DataProductAssetCount,0) AS DataProductAssetCount
            |,1 AS DataProductCounter
            |,COALESCE(CA.ClassifiedAssetCount,0) AS ClassifiedAssetCount
            |,CASE
            |     WHEN COALESCE(AC.DataProductAssetCount,0) =0 THEN 0
            |	   ELSE CAST(COALESCE(CA.ClassifiedAssetCount, 0) AS DECIMAL) / COALESCE(AC.DataProductAssetCount, 0)
            |		 END AS DataProductClassificationScore
            |,CASE
            |    WHEN COALESCE(CA.ClassifiedAssetCount,0)>=1 THEN 1 ELSE 0
            |		END AS DataProductIsClassified
            |,COALESCE(DPHAWC.DataProductDataAssetHasOwnerCount,0) AS DataProductDataAssetHasOwnerCount
            |,COALESCE(DPO.DataProductHasOwner,0) AS DataProductHasOwner
            |,CAST(COALESCE(DPO.DataProductHasOwner,0) AS DECIMAL) / 1 AS DataProductOwnerScore
            |, CASE WHEN COALESCE(LENGTH(DP.DataProductDescription),0)>=100 THEN 1 ELSE 0 END AS DataProductDescHasLenGr100Char
            |, CASE WHEN COALESCE(LENGTH(BD.BusinessDomainDescription),0)>=100 THEN 1 ELSE 0 END AS DataProductAssoBusinessDomainDescHasLenGr100Char
            |, COALESCE(GTDP.DPAssoAllTremDescHasLenGr25Char,0) AS DataProductAssoAllTremDescHasLenGr25Char
            |, CAST(CASE WHEN COALESCE(LENGTH(DP.DataProductDescription),0)>=100 THEN 1 ELSE 0 END
            |  + CASE WHEN COALESCE(LENGTH(BD.BusinessDomainDescription),0)>=100 THEN 1 ELSE 0 END
            |  + COALESCE(GTDP.DPAssoAllTremDescHasLenGr25Char,0) AS DECIMAL)/3
            |  AS DataProductUsabilityScore
            | FROM DataProduct DP
            | LEFT JOIN DataProductBusinessDomainAssignment DPBDA
            | ON DP.DataProductId = DPBDA.DataProductId
            | LEFT JOIN BusinessDomain BD
            | ON DPBDA.BusinessDomainId = BD.BusinessDomainId
            | LEFT JOIN
            |			(SELECT DPAA.DataProductId, COUNT(DA.DataAssetId) DataProductAssetCount
            |			FROM DataAsset DA
            |			INNER JOIN DataProductAssetAssignment DPAA
            |			ON DA.DataAssetId=DPAA.DataAssetId
            |			GROUP BY DPAA.DataProductId) AC
            | ON AC.DataProductId = DP.DataProductId
            | LEFT JOIN
            |			(SELECT Q.DataProductId,COUNT(Q.DataAssetId) AS ClassifiedAssetCount FROM (
            |			SELECT DISTINCT DPAA.DataProductId, DPAA.DataAssetId
            |			FROM DataAsset DA
            |			INNER JOIN DataProductAssetAssignment DPAA
            |			ON DA.DataAssetId=DPAA.DataAssetId
            |			INNER JOIN DataAssetColumnClassificationAssignment DACCA
            |			ON DA.DataAssetId = DACCA.DataAssetId) Q
            |			GROUP BY Q.DataProductId) CA
            | ON CA.DataProductId = DP.DataProductId
            | LEFT JOIN
            |(SELECT DPAA.DataProductId,COUNT(DAOA.DataAssetId) DataProductDataAssetHasOwnerCount
            | FROM DataProductAssetAssignment DPAA
            | INNER JOIN DataAssetOwnerAssignment DAOA
            | ON DPAA.DataAssetId = DAOA.DataAssetId
            | GROUP BY DPAA.DataProductId) DPHAWC
            | ON DP.DataProductId = DPHAWC.DataProductId
            | LEFT JOIN (
            |SELECT GTA.DataProductId,CASE WHEN MIN(LENGTH(GT.GlossaryDescription))>=25 THEN 1 ELSE 0 END AS DPAssoAllTremDescHasLenGr25Char
            |FROM GlossaryTermDataProductAssignment GTA
            |INNER JOIN GlossaryTerm GT
            |ON GT.GlossaryTermId = GTA.GlossaryTermId
            |GROUP BY GTA.DataProductId) GTDP
            |ON DP.DataProductId = GTDP.DataProductId
            | LEFT JOIN
            |(SELECT DISTINCT DataProductId, 1 AS DataProductHasOwner
            | FROM DataProductOwner) DPO
            | ON DP.DataProductId = DPO.DataProductId
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
          lit(null).cast(IntegerType).as("DataProductPublishedCount"),
          lit(null).cast(IntegerType).as("DataProductDraftCount"),
          lit(null).cast(IntegerType).as("DataProductExpiredCount"),
          lit(null).cast(IntegerType).as("DataProductHasDescription"),
          lit(null).cast(IntegerType).as("DataProductHasAsset"),
          lit(null).cast(IntegerType).as("DataProductHasPublishedGlossaryTerm"),
          lit(null).cast(IntegerType).as("DataProductHasGlossaryTerm"),
          lit(null).cast(IntegerType).as("DataProductHasUseCase"),
          lit(null).cast(DoubleType).as("DataProductDataCatalogingScore"),
          lit(null).cast(IntegerType).as("DataProductPublishedByAuthorizedUserCount"),
          lit(null).cast(IntegerType).as("DataProductAssetMDQEnabledCount"),
          lit(null).cast(IntegerType).as("DataProductAssetMDQNotEnabledCount"),
          lit(null).cast(DoubleType).as("DataProductAssetMDQEnabledScore"),
          lit(null).cast(IntegerType).as("DataProductCertifiedCount"),
          lit(null).cast(IntegerType).as("DataProductAccessGrantCount"),
          lit(null).cast(IntegerType).as("DataProductInSLAAccessGrantCount"),
          lit(null).cast(DoubleType).as("DataProductAccessGrantScore"),
          col("DataProductDescHasLenGr100Char").cast(IntegerType),
          col("DataProductAssoBusinessDomainDescHasLenGr100Char").cast(IntegerType),
          col("DataProductAssoAllTremDescHasLenGr25Char").cast(IntegerType),
          col("DataProductUsabilityScore").cast(DoubleType),
          lit(null).cast(IntegerType).as("DataProductHasParentBusinessDomain"),
          lit(null).cast(IntegerType).as("DataProductHasAtLeast1Asset"),
          lit(null).cast(IntegerType).as("DataProductHasAssoPubTerm"),
          lit(null).cast(DoubleType).as("LinkedAssetsScore"),
          lit(null).cast(IntegerType).as("DataProductAllAssoAssetsHasOwners"),
          lit(null).cast(DoubleType).as("OwnershipScore")
        )
        val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema = schema)
        dfProcessed
      }
      else
      {
        spark.emptyDataFrame
      }
    } catch {
      case e: Exception =>
        println(s"Error Processing FactDataGovernanceScanControlDataProductUsabilityMetadataQualityManagement Control: ${e.getMessage}")
        logger.error(s"Error Processing FactDataGovernanceScanControlDataProductUsabilityMetadataQualityManagement Control: ${e.getMessage}")
        throw e
    }
  }
}
