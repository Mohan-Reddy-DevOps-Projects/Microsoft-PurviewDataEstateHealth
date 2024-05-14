package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SaveMode, SparkSession}
import io.delta.tables.DeltaTable

class Reader(spark: SparkSession, logger: Logger) {
  private def deltaTableExists(deltaTablePath: String): Boolean = {
    try {
      DeltaTable.isDeltaTable(deltaTablePath)
    } catch {
      case _: Throwable => false
    }
  }

  def readAdlsDelta(DeltaPath: String, Entity: String): Option[DataFrame] = {
    val directoryPath = DeltaPath.concat("/DomainModel/").concat(Entity)
    val DomainDeltaTable = DeltaTable.forPath(spark, directoryPath)
    val dfDomainDeltaTable = DomainDeltaTable.toDF
    if (deltaTableExists(directoryPath) && !dfDomainDeltaTable.isEmpty) {
      val df = dfDomainDeltaTable
      Some(df)
    } else {
      println(s"Delta table does not exist at path or IsEmpty: $directoryPath")
      None
    }
  }
  def writeCosmosData(df:DataFrame,Entity:String): Unit = {
    try {
      df.write.format("cosmos.oltp")
        .option("spark.cosmos.accountEndpoint", spark.conf.get("spark.cosmos.accountEndpoint"))
        .option("spark.cosmos.database", spark.conf.get("spark.cosmos.database"))
        .option("spark.cosmos.container", "dehsentinel")
        .option("spark.cosmos.accountKey", spark.conf.get("spark.cosmos.accountKey"))
        .mode(SaveMode.Append)
        .save()
    }
    catch {
      case e: Exception =>
        println(s"Error Checkpointing 'dehsentinel' Asset $Entity: ${e.getMessage}")
        logger.error(s"Error Checkpointing 'dehsentinel' Asset $Entity: ${e.getMessage}")
        throw e
    }
  }
}
