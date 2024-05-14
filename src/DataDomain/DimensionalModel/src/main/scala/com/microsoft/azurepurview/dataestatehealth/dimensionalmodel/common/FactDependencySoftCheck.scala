package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import org.apache.log4j.Logger
import org.apache.spark.sql.SparkSession
import io.delta.tables.DeltaTable

class FactDependencySoftCheck (spark: SparkSession, logger: Logger){
  def checkAssets(ProcessingAssetName:String,adlsRootPath: String, listOfAssets: List[String]): Boolean = {
    try {
      listOfAssets.foreach { assetName =>
        val assetPath = adlsRootPath + "/" + assetName
        val assetDeltaTable = DeltaTable.forPath(spark, assetPath)
        val dfAssetDeltaTable = assetDeltaTable.toDF
        if (DeltaTable.isDeltaTable(assetPath) && !dfAssetDeltaTable.isEmpty) {
          println(s"ProcessingAssetName '$ProcessingAssetName' FactDependencySoftCheck Delta table '$assetName' exists at '$assetPath'")
        } else {
          throw new IllegalArgumentException(s"Asset path does not exist or IsEmpty: $assetPath")
        }
      }
      println(s"ProcessingAssetName '$ProcessingAssetName' FactDependencySoftCheck All Dependent Delta table exists! Check Successful!")
      true
    } catch {
      case e: Exception =>
        println(s"ProcessingAssetName '$ProcessingAssetName' Error processing assets: ${e.getMessage}")
        println(s"ProcessingAssetName '$ProcessingAssetName' FactDependencySoftCheck All Dependent Delta table DO NOT Exist! Check Failed!")
        false
    }
  }

}
