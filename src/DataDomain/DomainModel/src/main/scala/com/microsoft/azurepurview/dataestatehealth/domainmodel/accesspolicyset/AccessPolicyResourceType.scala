package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.col

class AccessPolicyResourceType (spark: SparkSession, logger:Logger){
  def processAccessPolicyResourceType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("ResourceTypeId")
        ,col("ResourceType").alias("ResourceTypeDisplayName"))
        .filter("OperationType=='Create' or OperationType=='Update'")
        .distinct()

      dfProcess = dfProcess.filter("ResourceTypeId IS NOT NULL").distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing AccessPolicyResourceType Data: ${e.getMessage}")
        logger.error(s"Error Processing AccessPolicyResourceType Data: ${e.getMessage}")
        throw e
    }
  }
}