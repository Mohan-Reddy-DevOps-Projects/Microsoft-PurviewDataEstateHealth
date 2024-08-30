package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, _}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
class Relationship (spark: SparkSession, logger:Logger){
  def processRelationship(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(col("accountId").alias("AccountId").cast(StringType)
        ,col("payload.after.Type").alias("Type").cast(StringType)
        ,col("payload.after.SourceType").alias("SourceType").cast(StringType)
        ,col("payload.after.SourceId").alias("SourceId").cast(StringType)
        ,col("payload.after.TargetType").alias("TargetType").cast(StringType)
        ,col("payload.after.TargetId").alias("TargetId").cast(StringType)
        ,col("changedBy").alias("ModifiedByUserId").cast(StringType)
        ,col("preciseTimestamp").alias("ModifiedDateTime").cast(TimestampType)
        ,col("_ts").alias("EventProcessingTime").cast(LongType)
        ,col("operationType").alias("OperationType").cast(StringType)).filter("operationType=='Create' or operationType=='Update'")
      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
      val dfProcessDelete = df.select(col("accountId").alias("AccountId").cast(StringType)
        ,col("payload.before.Type").alias("Type").cast(StringType)
        ,col("payload.before.SourceType").alias("SourceType").cast(StringType)
        ,col("payload.before.SourceId").alias("SourceId").cast(StringType)
        ,col("payload.before.TargetType").alias("TargetType").cast(StringType)
        ,col("payload.before.TargetId").alias("TargetId").cast(StringType)
        ,col("changedBy").alias("ModifiedByUserId").cast(StringType)
        ,col("preciseTimestamp").alias("ModifiedDateTime").cast(TimestampType)
        ,col("_ts").alias("EventProcessingTime").cast(LongType)
        ,col("operationType").alias("OperationType").cast(StringType)).filter("operationType=='Delete'")
      dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      val windowSpec = Window.partitionBy("Type","SourceType","SourceId","TargetType","TargetId")
        .orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      dfProcess = dfProcess.filter(s"""Type IS NOT NULL
                                 | AND SourceType IS NOT NULL
                                 | AND SourceId IS NOT NULL
                                 | AND TargetType IS NOT NULL
                                 | AND TargetId IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""Type is null
                            | or SourceType is null
                            | or SourceId is null
                            | or TargetType is null
                            | or TargetId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing Relationship Data: ${e.getMessage}")
        logger.error(s"Error Processing Relationship Data: ${e.getMessage}")
        throw e
    }
  }
}
