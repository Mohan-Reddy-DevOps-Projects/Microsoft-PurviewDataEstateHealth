package com.microsoft.azurepurview.dataestatehealth.domainmodel.common

import org.apache.log4j.Logger
import org.apache.spark.sql.SparkSession

class ColdStartSoftCheck(spark: SparkSession, logger: Logger) {
  def validateCheckIn(cosmosDBLinkedServiceName: String, containerName: String
                      , database:String=spark.conf.get("spark.cosmos.database")): Boolean = {
    try {
      //Soft Check to Overcome the cold Start Problem
      val df = spark.read.format("cosmos.olap")
        //.option("spark.synapse.linkedService", cosmosDBLinkedServiceName)
        .option("spark.cosmos.accountEndpoint", spark.conf.get("spark.cosmos.accountEndpoint"))
        .option("spark.cosmos.database", database)
        .option("spark.cosmos.container", containerName)
        .option("spark.cosmos.accountKey", spark.conf.get("spark.cosmos.accountKey"))
        .load()

      !df.isEmpty
    } catch {
      case ex: Exception =>
        logger.error(s"Error reading data from Cosmos DB: ${ex.getMessage}")
        false // Return false indicating failure
    }
  }
}

