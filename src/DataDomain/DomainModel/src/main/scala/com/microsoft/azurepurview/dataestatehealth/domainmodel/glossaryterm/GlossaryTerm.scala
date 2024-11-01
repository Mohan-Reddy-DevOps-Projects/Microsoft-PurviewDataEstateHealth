package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, Validator}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, _}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import io.delta.tables.DeltaTable
import org.apache.spark.sql.expressions.Window

import java.sql.Timestamp

class GlossaryTerm (spark: SparkSession, logger:Logger){
  def processGlossaryTerm(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("GlossaryTermId")
        ,col("payload.after.name").alias("GlossaryTermDisplayName")
        ,col("payload.after.status").alias("Status")
        ,col("payload.after.description").alias("GlossaryDescription")
        ,col("payload.after.systemData.createdBy").alias("CreatedByUserId")
        ,col("payload.after.systemData.createdAt").alias("CreatedDatetime")
        ,col("payload.after.systemData.lastModifiedBy").alias("ModifiedByUserId")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
        ,col("payload.after.parentId").alias("ParentGlossaryTermId")
        ,col("payload.after.domain").alias("BusinessDomainId")).filter("operationType=='Create' or operationType=='Update'")
      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.id").alias("GlossaryTermId")
          ,col("payload.before.name").alias("GlossaryTermDisplayName")
          ,col("payload.before.status").alias("Status")
          ,col("payload.before.description").alias("GlossaryDescription")
          ,col("payload.before.systemData.createdBy").alias("CreatedByUserId")
          ,col("payload.before.systemData.createdAt").alias("CreatedDatetime")
          ,col("payload.before.systemData.lastModifiedBy").alias("ModifiedByUserId")
          ,col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime")
          ,col("payload.before.parentId").alias("ParentGlossaryTermId")
          ,col("payload.before.domain").alias("BusinessDomainId")).filter("operationType=='Delete'")

        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      val windowSpecGT = Window.partitionBy("GlossaryTermId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc,
          when(col("OperationType") === "Create", 1)
            .when(col("OperationType") === "Update", 2)
            .when(col("OperationType") === "Delete", 3)
            .otherwise(4)
            .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpecGT))
        .filter(col("row_number") === 1)
        .drop("row_number")

      dfProcess = dfProcess
        .withColumn("IsLeaf", lit(null: IntegerType))

      dfProcess = dfProcess.filter("GlossaryTermId IS NOT NULL AND GlossaryTermDisplayName IS NOT NULL").distinct()

      dfProcess = dfProcess.select(col("GlossaryTermId").cast(StringType)
        ,col("ParentGlossaryTermId").cast(StringType)
        ,col("GlossaryTermDisplayName").cast(StringType)
        ,col("GlossaryDescription").cast(StringType)
        ,col("AccountId").cast(StringType)
        ,col("Status").cast(StringType)
        ,col("IsLeaf").cast(IntegerType)
        ,col("CreatedDatetime").cast(TimestampType)
        ,col("CreatedByUserId").cast(StringType)
        ,col("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType").cast(StringType)
        ,col("BusinessDomainId").cast(StringType)
      )
      val windowSpec = Window.partitionBy("GlossaryTermId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString =
        s"""GlossaryTermId is null
           | or GlossaryTermDisplayName is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing GlossaryTerm Data: ${e.getMessage}")
        logger.error(s"Error Processing GlossaryTerm Data: ${e.getMessage}")
        throw e
    }
  }
}
