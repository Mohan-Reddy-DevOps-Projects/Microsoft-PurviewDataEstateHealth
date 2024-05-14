package com.microsoft.azurepurview.dataestatehealth.domainmodel.common
import org.apache.log4j.Logger
import org.apache.spark.sql.types.StructType
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.SaveMode
class Reader (spark: SparkSession, logger:Logger){
  def readCosmosData(contractSchema:StructType,cosmosDBLinkedServiceName:String,accountId:String,container:String,eventSource:String,payloadKind:String): DataFrame = {
    try {
      var df = spark.read.format("cosmos.olap")
        .schema(contractSchema)
        //.option("spark.synapse.linkedService", cosmosDBLinkedServiceName)
        .option("spark.cosmos.accountEndpoint", spark.conf.get("spark.cosmos.accountEndpoint"))
        .option("spark.cosmos.database", spark.conf.get("spark.cosmos.database"))
        .option("spark.cosmos.container", container)
        .option("spark.cosmos.accountKey", spark.conf.get("spark.cosmos.accountKey"))
        .load().filter(s"accountId = '$accountId'")
        .filter(s"payloadKind = '$payloadKind'")
      if (eventSource.nonEmpty) {
        df = df.filter(s"eventSource = '$eventSource'")
      }
      df
    }
    catch {
      case e: Exception =>
        println(s"Error reading $container data: ${e.getMessage}")
        logger.error(s"Error reading $container data: ${e.getMessage}")
        throw e
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