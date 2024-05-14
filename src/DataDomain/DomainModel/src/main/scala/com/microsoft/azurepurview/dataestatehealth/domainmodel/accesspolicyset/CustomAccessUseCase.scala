package com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col}

class CustomAccessUseCase (spark: SparkSession, logger:Logger){
  def processCustomAccessUseCase(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("AccessPolicySetId")
        ,col("AccessUseCaseDisplayName")
        ,col("AccessUseCaseDescription"))
        .filter("OperationType=='Create' or OperationType=='Update'")
        .filter("AccessUseCaseDisplayName IS NOT NULL")
        .distinct()

      dfProcess = dfProcess.filter("AccessPolicySetId IS NOT NULL").distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing CustomAccessUseCase Data: ${e.getMessage}")
        logger.error(s"Error Processing CustomAccessUseCase Data: ${e.getMessage}")
        throw e
    }
  }

}
