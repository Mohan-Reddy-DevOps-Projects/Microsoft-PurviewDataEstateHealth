package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.apache.spark.sql.types._

/**
 * Schema definition for Control Job contracts
 */
class ControlJobContractSchema {
  
  /**
   * Defines the schema for control job contracts in Cosmos DB
   *
   * @return The StructType representing the schema
   */
  def controlJobContractSchema: StructType = {
    // Define nested schema for rules
    StructType(Seq(
      StructField("id", StringType, true),
      StructField("tenantId", StringType, true),
      StructField("accountId", StringType, true),
      StructField("systemData", StructType(Seq(
        StructField("createdBy", StringType, true),
        StructField("createdAt", StringType, true),
        StructField("lastModifiedBy", StringType, true),
        StructField("lastModifiedAt", StringType, true)
      )), true),
      StructField("type", StringType, true),
      StructField("jobId", StringType, true),
      StructField("createdTime", StringType, true),
      StructField("jobStatus", StringType, true),
      StructField("storageEndpoint", StringType, true),
      StructField("inputs", ArrayType(StructType(Seq(
        StructField("type", StringType, true),
        StructField("typeProperties", StructType(Seq(
          StructField("fileSystem", StringType, true),
          StructField("folderPath", StringType, true),
          StructField("datasourceFQN", StringType, true)
        )), true)
      ))), true),
      StructField("evaluations", ArrayType(StructType(Seq(
        StructField("controlId", StringType, true),
        StructField("query", StringType, true),
        StructField("rules", ArrayType(StructType(Seq(
          StructField("id", StringType, true),
          StructField("name", StringType, true),
          StructField("type", StringType, true),
          StructField("condition", StringType, true)
        ))), true)
      ))), true)
    ))
  }
}