package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{GenerateId, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, explode_outer}
import org.apache.spark.sql.types.StringType

class DataQualityRuleType (spark: SparkSession, logger:Logger){
  def processDataQualityRuleType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      var dfProcess = df.select(col("payloadDetails.domainmodel.payload.dataQualityFact").alias("DataQualityFact"))
        .filter("payloadDetails.domainmodel.eventSource=='DataQuality'")
        .filter("payloadDetails.domainmodel.payloadKind=='dataQualityFact'")
        .filter("payloadDetails.domainmodel.operationType=='upsert'")

      dfProcess = dfProcess.withColumn("DataQualityFact_exploded",explode_outer(col("DataQualityFact")))
      dfProcess = dfProcess.withColumn("RuleTypeDisplayName",col("DataQualityFact_exploded.ruleType"))
        .withColumn("Dimension",col("DataQualityFact_exploded.dimension"))
        .drop("DataQualityFact_exploded")
        .drop("DataQualityFact")

      dfProcess = dfProcess.filter("RuleTypeDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("RuleTypeDisplayName","Dimension"),"RuleTypeId")

      dfProcess = dfProcess.select(col("RuleTypeId").cast(StringType)
        ,col("RuleTypeDisplayName").cast(StringType)
        ,col("RuleTypeDisplayName").alias("RuleTypeDesc").cast(StringType)
        ,col("Dimension").alias("DimensionDisplayName").cast(StringType))
        .distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""RuleTypeId is null
                            | or RuleTypeDisplayName is null
                            | or RuleTypeDesc is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataQualityRuleType Data: ${e.getMessage}")
        logger.error(s"Error Processing DataQualityRuleType Data: ${e.getMessage}")
        throw e
    }
  }
}