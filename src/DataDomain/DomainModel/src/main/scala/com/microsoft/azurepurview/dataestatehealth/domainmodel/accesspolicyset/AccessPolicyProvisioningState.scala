package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset

import org.apache.log4j.Logger
import org.apache.spark.sql.functions.col
import org.apache.spark.sql.{DataFrame, SparkSession}

class AccessPolicyProvisioningState (spark: SparkSession, logger:Logger){
  def processAccessPolicyProvisioningState(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("ProvisioningStateId")
        ,col("ProvisioningStateDisplayName"))
        .filter("OperationType=='Create' or OperationType=='Update'")
        .distinct()

      dfProcess = dfProcess.filter("ProvisioningStateId IS NOT NULL").distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing AccessPolicyProvisioningState Data: ${e.getMessage}")
        logger.error(s"Error Processing AccessPolicyProvisioningState Data: ${e.getMessage}")
        throw e
    }
  }
}