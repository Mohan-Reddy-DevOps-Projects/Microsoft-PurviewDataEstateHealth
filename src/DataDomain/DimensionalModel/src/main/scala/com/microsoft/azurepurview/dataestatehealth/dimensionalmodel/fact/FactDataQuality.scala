package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{CreateAssetTempView, FactDependencySoftCheck, GenerateId}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.types.StructType

class FactDataQuality(spark: SparkSession, logger: Logger) {
  // SQL queries to get DQ and MDQ Results
  private val sqlColumn: String =
    """SELECT IQ.DQJobSourceId
      |,DRM.DQRuleId
      |,IQ.RuleScanCompletionDatetime
      |,IQ.DEHLastProcessedDatetime
      |,BD.BusinessDomainId
      |,DP.DataProductId
      |,DA.DataAssetId
      |,DAC.DataAssetColumnId
      |,DQJT.JobTypeId
      |,DRT.DQRuleTypeId
      |,IQ.DQScanProfileId
      |,ScanCompletionDateId
      |,IQ.DQOverallProfileQualityScore
      |,IQ.DQPassedCount
      |,IQ.DQFailedCount
      |,IQ.DQIgnoredCount
      |,IQ.DQEmptyCount
      |,IQ.DQMiscastCount
      | FROM
      |(
      | SELECT
      | J.JobExecutionId AS DQJobSourceId
      |,DQCol.RuleId AS DQRuleId
      |,J.ExecutionEndDateTime AS RuleScanCompletionDatetime
      |,CURRENT_TIMESTAMP() As DEHLastProcessedDatetime
      |,COALESCE(BD.BusinessDomainSourceId,(SELECT BusinessDomainSourceId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainSourceId
      |,COALESCE(DP.DataProductSourceId,(SELECT DataProductSourceId FROM DimDataProduct WHERE DataProductDisplayName='NOT-AVAILABLE')) AS DataProductSourceId
      |,COALESCE(DA.DataAssetSourceId,(SELECT DataAssetSourceId FROM DimDataAsset WHERE DataAssetDisplayName='NOT-AVAILABLE')) AS DataAssetSourceId
      |,COALESCE(DC.DataAssetColumnSourceId,(SELECT DataAssetColumnSourceId FROM DimDataAssetColumn WHERE DataAssetColumnDisplayName='NOT-AVAILABLE')) AS DataAssetColumnSourceId
      |,DQRT.RuleTypeId AS DQRuleTypeSourceId
      |,(SELECT  DQScanProfileId FROM DimDQScanProfile WHERE LOWER(RuleOriginDisplayName)='asset' and LOWER(RuleAppliedOn)='column') AS DQScanProfileId
      |,DQCol.ColumnResultScore AS DQOverallProfileQualityScore
      |,DQCol.RowPassCount AS DQPassedCount
      |,DQCol.RowFailCount AS DQFailedCount
      |,DQCol.RowIgnoredCount AS DQIgnoredCount
      |,DQCol.RowEmptyCount AS DQEmptyCount
      |,DQCol.RowMiscastCount AS DQMiscastCount
      |,DD.DateId AS ScanCompletionDateId
      |,COALESCE(J.JobType, 'NOT-AVAILABLE') AS JobType
      | FROM DataQualityRuleColumnExecution DQCol
      | INNER JOIN DataQualityJobExecution J ON DQCol.JobExecutionId=J.JobExecutionId
      | INNER JOIN DataQualityRule DQR ON DQCol.RuleId = DQR.RuleId
      | INNER JOIN DataQualityRuleType DQRT ON DQRT.RuleTypeId = DQR.RuleTypeId
      | INNER JOIN DimDate DD ON CAST(DATE_FORMAT(ExecutionEndDateTime, 'yyyyMMdd') AS INT)=DD.DateId
      | LEFT OUTER JOIN DimBusinessDomain BD ON DQR.BusinessDomainId = BD.BusinessDomainSourceId AND BD.CurrentIndicator=1
      | LEFT OUTER JOIN DimDataProduct DP ON DQR.DataProductId = DP.DataProductSourceId  AND DP.IsActive = true
      | LEFT OUTER JOIN DimDataAsset DA ON DQCol.DataAssetId = DA.DataAssetSourceId AND DA.CurrentIndicator = 1
      | LEFT OUTER JOIN DimDataAssetColumn DC ON DQCol.ColumnId = DC.DataAssetColumnSourceId AND DC.CurrentIndicator = 1
      | WHERE LOWER(DQR.RuleOriginDisplayName)='asset' AND LOWER(DQR.RuleTargetObjectType)='column'
      |) IQ
      | INNER JOIN DimDQRuleName DRM ON DRM.DQRuleNameId = IQ.DQRuleId
      | INNER JOIN DimBusinessDomain BD ON IQ.BusinessDomainSourceId=BD.BusinessDomainSourceId
      | INNER JOIN DimDataProduct DP ON IQ.DataProductSourceId=DP.DataProductSourceId
      | INNER JOIN DimDataAsset DA ON IQ.DataAssetSourceId=DA.DataAssetSourceId
      | INNER JOIN DimDataAssetColumn DAC ON IQ.DataAssetColumnSourceId=DAC.DataAssetColumnSourceId
      | INNER JOIN DimDQRuleType DRT ON DRT.DQRuleTypeSourceId = IQ.DQRuleTypeSourceId
      | INNER JOIN DimDQJobType DQJT ON LOWER(DQJT.JobTypeDisplayName) = LOWER(IQ.JobType)
      | UNION ALL
      | SELECT IQ.DQJobSourceId
      |,DRM.DQRuleId
      |,IQ.RuleScanCompletionDatetime
      |,IQ.DEHLastProcessedDatetime
      |,BD.BusinessDomainId
      |,DP.DataProductId
      |,DA.DataAssetId
      |,DAC.DataAssetColumnId
      |,DQJT.JobTypeId
      |,DRT.DQRuleTypeId
      |,IQ.DQScanProfileId
      |,ScanCompletionDateId
      |,IQ.DQOverallProfileQualityScore
      |,IQ.DQPassedCount
      |,IQ.DQFailedCount
      |,IQ.DQIgnoredCount
      |,IQ.DQEmptyCount
      |,IQ.DQMiscastCount
      | FROM
      |(
      | SELECT
      | J.JobExecutionId AS DQJobSourceId
      |,DQCol.RuleId AS DQRuleId
      |,J.ExecutionEndDateTime AS RuleScanCompletionDatetime
      |,CURRENT_TIMESTAMP() As DEHLastProcessedDatetime
      |,COALESCE(BD.BusinessDomainSourceId,(SELECT BusinessDomainSourceId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainSourceId
      |,COALESCE(DP.DataProductSourceId,(SELECT DataProductSourceId FROM DimDataProduct WHERE DataProductDisplayName='NOT-AVAILABLE')) AS DataProductSourceId
      |,COALESCE(DA.DataAssetSourceId,(SELECT DataAssetSourceId FROM DimDataAsset WHERE DataAssetDisplayName='NOT-AVAILABLE')) AS DataAssetSourceId
      |,COALESCE(DC.DataAssetColumnSourceId,(SELECT DataAssetColumnSourceId FROM DimDataAssetColumn WHERE DataAssetColumnDisplayName='NOT-AVAILABLE')) AS DataAssetColumnSourceId
      |,DQRT.RuleTypeId AS DQRuleTypeSourceId
      |,(SELECT  DQScanProfileId FROM DimDQScanProfile WHERE LOWER(RuleOriginDisplayName)='cde' and LOWER(RuleAppliedOn)='column') AS DQScanProfileId
      |,DQCol.ColumnResultScore AS DQOverallProfileQualityScore
      |,DQCol.RowPassCount AS DQPassedCount
      |,DQCol.RowFailCount AS DQFailedCount
      |,DQCol.RowIgnoredCount AS DQIgnoredCount
      |,DQCol.RowEmptyCount AS DQEmptyCount
      |,DQCol.RowMiscastCount AS DQMiscastCount
      |,DD.DateId AS ScanCompletionDateId
      |,COALESCE(J.JobType, 'NOT-AVAILABLE') AS JobType
      | FROM DataQualityRuleColumnExecution DQCol
      | INNER JOIN DataQualityJobExecution J ON DQCol.JobExecutionId=J.JobExecutionId
      | INNER JOIN DataQualityRule DQR ON DQCol.RuleId = DQR.RuleId
      | INNER JOIN DataQualityRuleType DQRT ON DQRT.RuleTypeId = DQR.RuleTypeId
      | INNER JOIN DimDate DD ON CAST(DATE_FORMAT(ExecutionEndDateTime, 'yyyyMMdd') AS INT)=DD.DateId
      | LEFT OUTER JOIN DimBusinessDomain BD ON DQR.BusinessDomainId = BD.BusinessDomainSourceId AND BD.CurrentIndicator=1
      | LEFT OUTER JOIN DimDataProduct DP ON DQR.DataProductId = DP.DataProductSourceId  AND DP.IsActive = true
      | LEFT OUTER JOIN DimDataAsset DA ON DQCol.DataAssetId = DA.DataAssetSourceId AND DA.CurrentIndicator = 1
      | LEFT OUTER JOIN DimDataAssetColumn DC ON DQCol.ColumnId = DC.DataAssetColumnSourceId AND DC.CurrentIndicator = 1
      | WHERE LOWER(DQR.RuleOriginDisplayName)='cde' AND LOWER(DQR.RuleTargetObjectType)='column'
      |) IQ
      | INNER JOIN DimDQRuleName DRM ON DRM.DQRuleNameId = IQ.DQRuleId
      | INNER JOIN DimBusinessDomain BD ON IQ.BusinessDomainSourceId=BD.BusinessDomainSourceId
      | INNER JOIN DimDataProduct DP ON IQ.DataProductSourceId=DP.DataProductSourceId
      | INNER JOIN DimDataAsset DA ON IQ.DataAssetSourceId=DA.DataAssetSourceId
      | INNER JOIN DimDataAssetColumn DAC ON IQ.DataAssetColumnSourceId=DAC.DataAssetColumnSourceId
      | INNER JOIN DimDQRuleType DRT ON DRT.DQRuleTypeSourceId = IQ.DQRuleTypeSourceId
      | INNER JOIN DimDQJobType DQJT ON LOWER(DQJT.JobTypeDisplayName) = LOWER(IQ.JobType)""".stripMargin

  private val sqlAsset: String =
    """SELECT IQ.DQJobSourceId
      |,DRM.DQRuleId
      |,IQ.RuleScanCompletionDatetime
      |,IQ.DEHLastProcessedDatetime
      |,BD.BusinessDomainId
      |,DP.DataProductId
      |,DA.DataAssetId
      |,DAC.DataAssetColumnId
      |,DQJT.JobTypeId
      |,DRT.DQRuleTypeId
      |,IQ.DQScanProfileId
      |,ScanCompletionDateId
      |,IQ.DQOverallProfileQualityScore
      |,IQ.DQPassedCount
      |,IQ.DQFailedCount
      |,IQ.DQIgnoredCount
      |,IQ.DQEmptyCount
      |,IQ.DQMiscastCount
      |
      |FROM
      |(
      | SELECT
      | J.JobExecutionId AS DQJobSourceId
      |,DQAsset.RuleId AS DQRuleId
      |,J.ExecutionEndDateTime AS RuleScanCompletionDatetime
      |,CURRENT_TIMESTAMP() As DEHLastProcessedDatetime
      |,COALESCE(BD.BusinessDomainSourceId,(SELECT BusinessDomainSourceId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainSourceId
      |,COALESCE(DP.DataProductSourceId,(SELECT DataProductSourceId FROM DimDataProduct WHERE DataProductDisplayName='NOT-AVAILABLE')) AS DataProductSourceId
      |,COALESCE(DA.DataAssetSourceId,(SELECT DataAssetSourceId FROM DimDataAsset WHERE DataAssetDisplayName='NOT-AVAILABLE')) AS DataAssetSourceId
      |,(SELECT DataAssetColumnSourceId FROM DimDataAssetColumn WHERE DataAssetColumnDisplayName='NOT-AVAILABLE') AS DataAssetColumnSourceId
      |,DQRT.RuleTypeId AS DQRuleTypeSourceId
      |,(SELECT  DQScanProfileId FROM DimDQScanProfile WHERE LOWER(RuleOriginDisplayName)='asset' and LOWER(RuleAppliedOn)='asset') AS DQScanProfileId
      |,DQAsset.AssetResultScore AS DQOverallProfileQualityScore
      |,DQAsset.RowPassCount AS DQPassedCount
      |,DQAsset.RowFailCount AS DQFailedCount
      |,DQAsset.RowIgnoredCount AS DQIgnoredCount
      |,DQAsset.RowEmptyCount AS DQEmptyCount
      |,DQAsset.RowMiscastCount AS DQMiscastCount
      |,DD.DateId AS ScanCompletionDateId
      |,COALESCE(J.JobType, 'NOT-AVAILABLE') AS JobType
      | FROM DataQualityAssetRuleExecution DQAsset
      | INNER JOIN DataQualityJobExecution J ON DQAsset.JobExecutionId=J.JobExecutionId
      | INNER JOIN DataQualityRule DQR ON DQAsset.RuleId = DQR.RuleId
      | INNER JOIN DataQualityRuleType DQRT ON DQRT.RuleTypeId = DQR.RuleTypeId
      | INNER JOIN DimDate DD ON CAST(DATE_FORMAT(ExecutionEndDateTime, 'yyyyMMdd') AS INT)=DD.DateId
      | LEFT OUTER JOIN DimBusinessDomain BD ON DQR.BusinessDomainId = BD.BusinessDomainSourceId AND BD.CurrentIndicator=1
      | LEFT OUTER JOIN DimDataProduct DP ON DQR.DataProductId = DP.DataProductSourceId  AND DP.IsActive = true
      | LEFT OUTER JOIN DimDataAsset DA ON DQAsset.DataAssetId = DA.DataAssetSourceId AND DA.CurrentIndicator = 1
      | WHERE LOWER(DQR.RuleOriginDisplayName)='asset' AND LOWER(DQR.RuleTargetObjectType)='asset'
      |) IQ
      | INNER JOIN DimDQRuleName DRM ON DRM.DQRuleNameId = IQ.DQRuleId
      | INNER JOIN DimBusinessDomain BD ON IQ.BusinessDomainSourceId=BD.BusinessDomainSourceId
      | INNER JOIN DimDataProduct DP ON IQ.DataProductSourceId=DP.DataProductSourceId
      | INNER JOIN DimDataAsset DA ON IQ.DataAssetSourceId=DA.DataAssetSourceId
      | INNER JOIN DimDataAssetColumn DAC ON IQ.DataAssetColumnSourceId=DAC.DataAssetColumnSourceId
      | INNER JOIN DimDQRuleType DRT ON DRT.DQRuleTypeSourceId = IQ.DQRuleTypeSourceId
      | INNER JOIN DimDQJobType DQJT ON LOWER(DQJT.JobTypeDisplayName) = LOWER(IQ.JobType)
      | UNION ALL
      | SELECT IQ.DQJobSourceId
      |,DRM.DQRuleId
      |,IQ.RuleScanCompletionDatetime
      |,IQ.DEHLastProcessedDatetime
      |,BD.BusinessDomainId
      |,DP.DataProductId
      |,DA.DataAssetId
      |,DAC.DataAssetColumnId
      |,DQJT.JobTypeId
      |,DRT.DQRuleTypeId
      |,IQ.DQScanProfileId
      |,ScanCompletionDateId
      |,IQ.DQOverallProfileQualityScore
      |,IQ.DQPassedCount
      |,IQ.DQFailedCount
      |,IQ.DQIgnoredCount
      |,IQ.DQEmptyCount
      |,IQ.DQMiscastCount
      |
      |FROM
      |(
      | SELECT
      | J.JobExecutionId AS DQJobSourceId
      |,DQAsset.RuleId AS DQRuleId
      |,J.ExecutionEndDateTime AS RuleScanCompletionDatetime
      |,CURRENT_TIMESTAMP() As DEHLastProcessedDatetime
      |,COALESCE(BD.BusinessDomainSourceId,(SELECT BusinessDomainSourceId FROM DimBusinessDomain WHERE BusinessDomainDisplayName='NOT-AVAILABLE')) AS BusinessDomainSourceId
      |,COALESCE(DP.DataProductSourceId,(SELECT DataProductSourceId FROM DimDataProduct WHERE DataProductDisplayName='NOT-AVAILABLE')) AS DataProductSourceId
      |,COALESCE(DA.DataAssetSourceId,(SELECT DataAssetSourceId FROM DimDataAsset WHERE DataAssetDisplayName='NOT-AVAILABLE')) AS DataAssetSourceId
      |,(SELECT DataAssetColumnSourceId FROM DimDataAssetColumn WHERE DataAssetColumnDisplayName='NOT-AVAILABLE') AS DataAssetColumnSourceId
      |,DQRT.RuleTypeId AS DQRuleTypeSourceId
      |,(SELECT  DQScanProfileId FROM DimDQScanProfile WHERE LOWER(RuleOriginDisplayName)='cde' and LOWER(RuleAppliedOn)='asset') AS DQScanProfileId
      |,DQAsset.AssetResultScore AS DQOverallProfileQualityScore
      |,DQAsset.RowPassCount AS DQPassedCount
      |,DQAsset.RowFailCount AS DQFailedCount
      |,DQAsset.RowIgnoredCount AS DQIgnoredCount
      |,DQAsset.RowEmptyCount AS DQEmptyCount
      |,DQAsset.RowMiscastCount AS DQMiscastCount
      |,DD.DateId AS ScanCompletionDateId
      |,COALESCE(J.JobType, 'NOT-AVAILABLE') AS JobType
      | FROM DataQualityAssetRuleExecution DQAsset
      | INNER JOIN DataQualityJobExecution J ON DQAsset.JobExecutionId=J.JobExecutionId
      | INNER JOIN DataQualityRule DQR ON DQAsset.RuleId = DQR.RuleId
      | INNER JOIN DataQualityRuleType DQRT ON DQRT.RuleTypeId = DQR.RuleTypeId
      | INNER JOIN DimDate DD ON CAST(DATE_FORMAT(ExecutionEndDateTime, 'yyyyMMdd') AS INT)=DD.DateId
      | LEFT OUTER JOIN DimBusinessDomain BD ON DQR.BusinessDomainId = BD.BusinessDomainSourceId AND BD.CurrentIndicator=1
      | LEFT OUTER JOIN DimDataProduct DP ON DQR.DataProductId = DP.DataProductSourceId  AND DP.IsActive = true
      | LEFT OUTER JOIN DimDataAsset DA ON DQAsset.DataAssetId = DA.DataAssetSourceId AND DA.CurrentIndicator = 1
      | WHERE LOWER(DQR.RuleOriginDisplayName)='cde' AND LOWER(DQR.RuleTargetObjectType)='asset'
      |) IQ
      | INNER JOIN DimDQRuleName DRM ON DRM.DQRuleNameId = IQ.DQRuleId
      | INNER JOIN DimBusinessDomain BD ON IQ.BusinessDomainSourceId=BD.BusinessDomainSourceId
      | INNER JOIN DimDataProduct DP ON IQ.DataProductSourceId=DP.DataProductSourceId
      | INNER JOIN DimDataAsset DA ON IQ.DataAssetSourceId=DA.DataAssetSourceId
      | INNER JOIN DimDataAssetColumn DAC ON IQ.DataAssetColumnSourceId=DAC.DataAssetColumnSourceId
      | INNER JOIN DimDQRuleType DRT ON DRT.DQRuleTypeSourceId = IQ.DQRuleTypeSourceId
      | INNER JOIN DimDQJobType DQJT ON LOWER(DQJT.JobTypeDisplayName) = LOWER(IQ.JobType)""".stripMargin

  def processFactDataQuality(schema: StructType, targetAdlsRootPath: String): DataFrame = {
    try {
      val factDependencySoftCheck = new FactDependencySoftCheck(spark, logger)

      // Define assets
      val (listOfDomainAssets, listOfDimensionalAssets) = defineAssets()

      // Check asset dependencies
      if (checkAssets(factDependencySoftCheck, targetAdlsRootPath, listOfDomainAssets, listOfDimensionalAssets)) {
        val tempViewProcessor = new CreateAssetTempView(spark, logger)

        // Process assets
        processAssets(tempViewProcessor, targetAdlsRootPath, listOfDomainAssets, listOfDimensionalAssets)

        // Get data quality result DataFrames
        val (dfProcessColumn, dfProcessAsset) = processDataQualityResults(tempViewProcessor, targetAdlsRootPath, factDependencySoftCheck)

        // Combine results
        val dfProcess = combineDataFrames(dfProcessColumn, dfProcessAsset)

        // Final processing
        finalizeDataFrame(dfProcess, schema)
      } else {
        spark.emptyDataFrame
      }

    } catch {
      case e: Exception =>
        logger.error(s"Error Processing FactDataQuality Dimension: ${e.getMessage}", e)
        throw e
    }
  }

  private def defineAssets(): (List[String], List[String]) = {
    val listOfDomainAssets = List(
      "DataQualityJobExecution",
      "DataQualityRule",
      "DataQualityRuleType"
    )

    val listOfDimensionalAssets = List(
      "DimBusinessDomain",
      "DimDataProduct",
      "DimDataAsset",
      "DimDataAssetColumn",
      "DimDQScanProfile",
      "DimDate",
      "DimDQRuleName",
      "DimDQRuleType",
      "DimDQJobType"
    )

    (listOfDomainAssets, listOfDimensionalAssets)
  }

  private def checkAssets(factDependencySoftCheck: FactDependencySoftCheck, targetAdlsRootPath: String,
                          listOfDomainAssets: List[String], listOfDimensionalAssets: List[String]): Boolean = {
    val domainCheck = factDependencySoftCheck.checkAssets("FactDataQuality", targetAdlsRootPath + "/DomainModel", listOfDomainAssets)
    val dimensionalCheck = factDependencySoftCheck.checkAssets("FactDataQuality", targetAdlsRootPath + "/DimensionalModel", listOfDimensionalAssets)
    domainCheck && dimensionalCheck
  }

  private def processAssets(tempViewProcessor: CreateAssetTempView, targetAdlsRootPath: String,
                            listOfDomainAssets: List[String], listOfDimensionalAssets: List[String]): Unit = {
    tempViewProcessor.processAssets(targetAdlsRootPath + "/DomainModel", listOfDomainAssets)
    tempViewProcessor.processAssets(targetAdlsRootPath + "/DimensionalModel", listOfDimensionalAssets)
  }

  private def processDataQualityResults(tempViewProcessor: CreateAssetTempView, targetAdlsRootPath: String,
                                        factDependencySoftCheck: FactDependencySoftCheck): (DataFrame, DataFrame) = {
    val listDQColumnRule = List("DataQualityRuleColumnExecution")
    val listDQAssetRule = List("DataQualityAssetRuleExecution")

    val isDQColumnRule = factDependencySoftCheck.checkAssets("FactDataQuality", targetAdlsRootPath + "/DomainModel", listDQColumnRule)
    val isDQAssetRule = factDependencySoftCheck.checkAssets("FactDataQuality", targetAdlsRootPath + "/DomainModel", listDQAssetRule)

    val dfProcessColumn = if (isDQColumnRule) {
      tempViewProcessor.processAssets(targetAdlsRootPath + "/DomainModel", listDQColumnRule)
      spark.sql(sqlColumn)
    } else {
      spark.emptyDataFrame
    }

    val dfProcessAsset = if (isDQAssetRule) {
      tempViewProcessor.processAssets(targetAdlsRootPath + "/DomainModel", listDQAssetRule)
      spark.sql(sqlAsset)
    } else {
      spark.emptyDataFrame
    }

    (dfProcessColumn, dfProcessAsset)
  }

  private def combineDataFrames(dfColumn: DataFrame, dfAsset: DataFrame): DataFrame = {
    (dfColumn, dfAsset) match {
      case (dq, mdq) if dq.isEmpty && mdq.isEmpty => spark.emptyDataFrame
      case (dq, mdq) if dq.isEmpty => mdq
      case (dq, mdq) if mdq.isEmpty => dq
      case _ => dfColumn.union(dfAsset)
    }
  }

  private def finalizeDataFrame(dfProcess: DataFrame, schema: StructType): DataFrame = {
    if (dfProcess.isEmpty) {
      spark.emptyDataFrame
    } else {
      val windowSpec = Window.orderBy("RuleScanCompletionDatetime")
      val processedDF = dfProcess.withColumn("FactDataQualityId", row_number().over(windowSpec))
        .select(
          col("FactDataQualityId"),
          col("DQJobSourceId"),
          col("DQRuleId"),
          col("RuleScanCompletionDatetime"),
          col("DEHLastProcessedDatetime"),
          col("BusinessDomainId"),
          col("DataProductId"),
          col("DataAssetId"),
          col("DataAssetColumnId"),
          col("JobTypeId"),
          col("DQRuleTypeId"),
          col("DQScanProfileId"),
          col("ScanCompletionDateId"),
          col("DQOverallProfileQualityScore"),
          col("DQPassedCount"),
          col("DQFailedCount"),
          col("DQIgnoredCount"),
          col("DQEmptyCount"),
          col("DQMiscastCount")
        )
      spark.createDataFrame(processedDF.rdd, schema = schema)
    }
  }
}
