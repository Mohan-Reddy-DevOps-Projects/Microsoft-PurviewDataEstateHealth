package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, explode_outer, expr, lit, lower, max, row_number, trim, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

import java.sql.Timestamp

class DataAssetColumn (spark: SparkSession, logger:Logger){
  def processDataAssetColumn(df:DataFrame,schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Frame DataAsset Dataframe
      var dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("DataAssetId")
        ,col("payload.after.name").alias("DataAssetName")
        ,col("payload.after.type").alias("DataAssetType")
        ,col("payload.after.schema").alias("ColumnSchema")
        ,col("payload.after.systemData.createdAt").alias("CreatedDatetime")
        ,col("payload.after.systemData.createdBy").alias("CreatedByUserId")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
        ,col("payload.after.systemData.lastModifiedBy").alias("ModifiedByUserId"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        var dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.id").alias("DataAssetId")
          ,col("payload.before.name").alias("DataAssetName")
          ,col("payload.before.type").alias("DataAssetType")
          ,col("payload.before.schema").alias("ColumnSchema")
          ,col("payload.after.systemData.createdAt").alias("CreatedDatetime")
          ,col("payload.after.systemData.createdBy").alias("CreatedByUserId")
          ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
          ,col("payload.after.systemData.lastModifiedBy").alias("ModifiedByUserId"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      val windowSpecAsset = Window.partitionBy("DataAssetId")
        .orderBy(coalesce(col("ModifiedDateTime").cast(TimestampType), lit(Timestamp.valueOf("2000-01-01 00:00:00"))).desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpecAsset))
        .filter(col("row_number") === 1)
        .drop("row_number")

      dfProcess = dfProcess.withColumn("afterExplodeColumnSchema", explode_outer(col("ColumnSchema")))
      dfProcess = dfProcess.withColumn("ColumnName",col("afterExplodeColumnSchema.name"))
        .withColumn("ColumnDataType",col("afterExplodeColumnSchema.type"))
        .withColumn("ColumnDescription",col("afterExplodeColumnSchema.description"))
        .withColumn("ColumnClassification",col("afterExplodeColumnSchema.classifications"))
        .drop("ColumnSchema")
        .drop("afterExplodeColumnSchema")

      dfProcess = dfProcess.withColumn("ExplodeColumnClassifications",explode_outer(col("ColumnClassification")))
        .drop("ColumnClassification")
        .withColumnRenamed("ExplodeColumnClassifications","ColumnClassification")
        .filter("ColumnName IS NOT NULL")

      dfProcess = dfProcess.filter(s"""DataAssetId IS NOT NULL
                                      | AND ColumnName IS NOT NULL
                                      | AND DataAssetType IS NOT NULL
                                      | AND ColumnDataType IS NOT NULL""".stripMargin).distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("ColumnName"),"ColumnId")
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataAssetId","ColumnId"),"ColumnId")
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataAssetType"),"DataAssetTypeId")
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("ColumnDataType"),"DataTypeId")
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("ColumnClassification"),"ClassificationId")

      dfProcess = dfProcess.select(col("DataAssetId").cast(StringType)
        ,col("ColumnId").cast(StringType)
        ,col("ColumnName").alias("ColumnDisplayName").cast(StringType)
        ,col("ColumnDescription").alias("ColumnDescription").cast(StringType)
        ,col("DataAssetTypeId").alias("DataAssetTypeId").cast(StringType)
        ,col("DataTypeId").alias("DataTypeId").cast(StringType)
        ,col("AccountId").alias("AccountId").cast(StringType)
        ,col("CreatedDatetime").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedByUserId").alias("CreatedByUserId").cast(StringType)
        ,col("ModifiedDateTime").alias("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").alias("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
        ,col("OperationType").alias("OperationType").cast(StringType)
        ,col("ClassificationId").alias("ClassificationId").cast(StringType)
        ,col("ColumnDataType").alias("ColumnDataType").cast(StringType)
        ,col("ColumnClassification").alias("ColumnClassification").cast(StringType)
      )
      val windowSpec = Window.partitionBy("DataAssetId","ColumnId","DataTypeId","ClassificationId").orderBy(col("ModifiedDateTime").desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)

      val filterString = s"""DataAssetId is null
        or ColumnId is null
        or DataTypeId is null
        or DataAssetTypeId is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)

      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetColumn Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetColumn Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataAssetColumn"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins))
        {
      var dfWrite = df.drop("ClassificationId")
        .drop("ColumnDataType")
        .drop("ColumnClassification")
        .distinct()

      val windowSpec = Window.partitionBy("DataAssetId","ColumnId").orderBy(col("ModifiedDateTime").desc)
      dfWrite = dfWrite.withColumn("row_number", row_number().over(windowSpec)).filter(col("row_number") === 1)
      .drop("row_number")

      if (refreshType=="full"){
        dfWrite.write
          .format("delta")
          .mode("overwrite")
          .save(adlsTargetDirectory.concat("/").concat(EntityName))}
      else if (refreshType=="incremental") {
        if (DeltaTable.isDeltaTable(adlsTargetDirectory.concat("/").concat(EntityName))) {
          val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/").concat(EntityName))
          val dfTarget = dfTargetDeltaTable.toDF
          if (!df.isEmpty && !dfTarget.isEmpty) {
            val maxEventProcessingTime = dfTarget.agg(max("EventProcessingTime")
              .as("maxEventProcessingTime"))
              .collect()(0)
              .getLong(0)

            val mergeDfSource = dfWrite
              .filter(lower(col("OperationType")) =!= "delete")
              .filter(col("EventProcessingTime") > maxEventProcessingTime)

            dfTargetDeltaTable.as("target")
              .merge(
                mergeDfSource.as("source"),
                """target.DataAssetId = source.DataAssetId AND target.ColumnId = source.ColumnId""")
              .whenMatched("source.ModifiedDatetime>target.ModifiedDatetime")
              .updateAll()
              .whenNotMatched()
              .insertAll()
              .execute()

            val deleteDfSource = dfWrite
              .filter(col("EventProcessingTime") > maxEventProcessingTime)
              .filter(lower(col("OperationType")) === "delete")

            dfTargetDeltaTable.as("target")
              .merge(
                deleteDfSource.as("source"),
                expr("target.DataAssetId = source.DataAssetId AND target.ColumnId = source.ColumnId")
              )
              .whenMatched
              .delete()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataAssetColumn Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            dfWrite.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataAssetColumn Does not exist for incremental merge. Performing Full Overwrite...")
          dfWrite.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataAssetColumn data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataAssetColumn data: ${e.getMessage}")
        throw e
    }
  }
}
