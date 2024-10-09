package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, explode_outer, expr, lit, row_number, when}
import org.apache.spark.sql.types.{DecimalType, IntegerType, StringType, TimestampType}

class DataQualityRuleColumnExecution (spark: SparkSession, logger:Logger){
  def processDataQualityRuleColumnExecution(df:DataFrame,schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{

      var dfProcess = df.select(col("payloadDetails.domainmodel.payload.dataQualityFact").alias("DataQualityFact"))
        .filter("payloadDetails.domainmodel.eventSource=='DataQuality'")
        .filter("payloadDetails.domainmodel.payloadKind=='dataQualityFact'")
        .filter("payloadDetails.domainmodel.operationType=='upsert'")

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
        .withColumn("JobExecutionId",col("DataQualityFact_exploded.jobExecutionId"))
        .withColumn("RuleId",col("DataQualityFact_exploded.ruleId"))
        .withColumn("Column",col("DataQualityFact_exploded.column"))
        .withColumn("ColumnResultScore",col("DataQualityFact_exploded.result"))
        .withColumn("RowPassCount",col("DataQualityFact_exploded.passed"))
        .withColumn("RowFailCount",col("DataQualityFact_exploded.failed"))
        .withColumn("RowMiscastCount",col("DataQualityFact_exploded.miscast"))
        .withColumn("RowEmptyCount",col("DataQualityFact_exploded.empty"))
        .withColumn("RowIgnoredCount",col("DataQualityFact_exploded.ignored"))
        .filter("DataQualityFact_exploded.isAppliedOn=='Column'")
        .drop("DataQualityFact_exploded")
        .drop("DataQualityFact")

      dfProcess = dfProcess.filter(s"""JobExecutionId IS NOT NULL
                                      | AND RuleId IS NOT NULL
                                      |  AND DataAssetId IS NOT NULL""".stripMargin).distinct()

      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("BusinessDomainId","DataProductId","DataAssetId","RuleId"),"RuleId")

      //Read dependent Table DataAssetColumn
      val dfDataAssetColumn=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/DataAssetColumn"))
      val joinCondition = "source.DataAssetId = dataAssetColumn.DataAssetId AND source.Column = dataAssetColumn.ColumnDisplayName"
      dfProcess = dfProcess.alias("source")
        .join(dfDataAssetColumn.alias("dataAssetColumn"), expr(joinCondition), "left_outer")
        .select(col("source.JobExecutionId").cast(StringType)
          ,col("source.RuleId").cast(StringType)
          ,col("source.DataAssetId").cast(StringType)
          ,col("dataAssetColumn.ColumnId").alias("ColumnId").cast(StringType)
          ,col("source.ColumnResultScore").cast(DecimalType(18, 10))
          ,col("source.RowPassCount").cast(IntegerType)
          ,col("source.RowFailCount").cast(IntegerType)
          ,col("source.RowMiscastCount").cast(IntegerType)
          ,col("source.RowEmptyCount").cast(IntegerType)
          ,col("source.RowIgnoredCount").cast(IntegerType))
        .distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""JobExecutionId is null
                            | or RuleId is null
                            | or DataAssetId is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataQualityRuleColumnExecution Data: ${e.getMessage}")
        logger.error(s"Error Processing DataQualityRuleColumnExecution Data: ${e.getMessage}")
        throw e
    }
  }
}
