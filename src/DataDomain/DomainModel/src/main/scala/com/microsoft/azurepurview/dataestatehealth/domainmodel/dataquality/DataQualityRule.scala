package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{GenerateId, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, explode_outer, lit, row_number,_}
import org.apache.spark.sql.types.{DecimalType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class DataQualityRule (spark: SparkSession, logger:Logger){
  def processDataQualityRule(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payloadDetails.domainmodel.payload.dataQualityFact").alias("DataQualityFact")
        ,col("payloadDetails.domainmodel.payload.systemData.resultedAt").alias("CreatedDatetime")
        ,col("payloadDetails.domainmodel.payload.systemData.resultedAt").alias("ModifiedDateTime"))
        .filter("payloadDetails.domainmodel.eventSource=='DataQuality'")
        .filter("payloadDetails.domainmodel.payloadKind=='dataQualityFact'")
      //.filter("payloadDetails.domainmodel.operationType=='upsert'")

      dfProcess = dfProcess.withColumn("DataQualityFact_exploded",explode_outer(col("DataQualityFact")))

      val generateIdColumn = new GenerateId()
      dfProcess = dfProcess
        //.withColumn("BusinessDomainId",col("DataQualityFact_exploded.businessDomainId"))
        .withColumn("BusinessDomainId",
          when(col("DataQualityFact_exploded.businessDomainId") === "___deh_business_domain_id___",
            generateIdColumn.generateUUID("___deh_business_domain_id___"))
            .otherwise(col("DataQualityFact_exploded.businessDomainId")))
        .withColumn("DataProductId",col("DataQualityFact_exploded.dataProductId"))
        .withColumn("DataAssetId",col("DataQualityFact_exploded.dataAssetId"))
        .withColumn("SourceRuleId",col("DataQualityFact_exploded.ruleId"))
        //.withColumn("RuleTypeId",col("DataQualityFact_exploded.ruleType"))
        .withColumn("RuleOriginDisplayName",col("DataQualityFact_exploded.ruleOrigin"))
        .withColumn("RuleTargetObjectType",col("DataQualityFact_exploded.isAppliedOn"))
        .withColumn("RuleTypeDisplayName",col("DataQualityFact_exploded.ruleType"))
        .withColumn("Dimension",col("DataQualityFact_exploded.dimension"))
        .withColumn("RuleDisplayName",lit(null:StringType))
        .withColumn("RuleDescription",lit(null:StringType))
        .withColumn("Status",lit(null:StringType))
        .withColumn("CreatedByUserId",lit(null:StringType))
        .withColumn("ModifiedByUserId",lit(null:StringType))
        .drop("DataQualityFact_exploded")
        .drop("DataQualityFact")

      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("BusinessDomainId","DataProductId","DataAssetId","SourceRuleId"),"RuleId")
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("RuleTypeDisplayName","Dimension"),"RuleTypeId")

      dfProcess = dfProcess.filter(s"""SourceRuleId IS NOT NULL
                                      | AND BusinessDomainId IS NOT NULL
                                      |  AND DataProductId IS NOT NULL
                                      |    AND DataAssetId IS NOT NULL
                                      |   AND RuleTypeId IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("RuleId").cast(StringType)
        ,col("SourceRuleId").cast(StringType)
        ,col("BusinessDomainId").cast(StringType)
        ,col("DataProductId").cast(StringType)
        ,col("DataAssetId").cast(StringType)
        ,col("RuleTypeId").cast(StringType)
        ,col("RuleOriginDisplayName").cast(StringType)
        ,col("RuleTargetObjectType").cast(StringType)
        ,col("RuleDisplayName").cast(StringType)
        ,col("RuleDescription").cast(StringType)
        ,col("Status").cast(StringType)
        ,col("AccountId").cast(StringType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("CreatedByUserId").cast(StringType)
        ,col("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType").cast(StringType))
        .distinct()

      val windowSpec = Window.partitionBy("RuleId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""RuleId is null
                            | or SourceRuleId is null
                            | or BusinessDomainId is null
                            | or DataProductId is null
                            | or AccountId is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataQualityRule Data: ${e.getMessage}")
        logger.error(s"Error Processing DataQualityRule Data: ${e.getMessage}")
        throw e
    }
  }
}
