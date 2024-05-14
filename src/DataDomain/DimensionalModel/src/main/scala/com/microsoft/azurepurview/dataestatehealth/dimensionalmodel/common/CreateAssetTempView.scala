package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common

import org.apache.log4j.Logger
import org.apache.spark.sql.SparkSession
import io.delta.tables.DeltaTable

class CreateAssetTempView (spark: SparkSession, logger: Logger){
  def processAssets(adlsRootPath: String, listOfAssets: List[String]): Unit = {
    try {
      listOfAssets.foreach { assetName =>
        val assetPath = adlsRootPath + "/" + assetName
        if (DeltaTable.isDeltaTable(assetPath)) {
          val assetDF = spark.read.format("delta").load(assetPath)
          assetDF.createOrReplaceTempView(assetName)
        } else {
          throw new IllegalArgumentException(s"Asset path does not exist: $assetPath")
        }
      }
    } catch {
      case e: Exception =>
        println(s"Error processing assets: ${e.getMessage}")
        throw e
    }
  }
}
