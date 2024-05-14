package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, lit, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._
class DataAsset (spark: SparkSession, logger:Logger){
  def processDataAsset(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      //Frame DataAsset Dataframe
      var dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("DataAssetId")
        ,col("payload.after.type").alias("DataAssetType")
        ,col("payload.after.name").alias("AssetDisplayName")
        ,col("payload.after.description").alias("AssetDescription")
        ,col("payload.after.source.fqn").alias("FullyQualifiedName")
        ,col("payload.after.source.type").alias("ScanSource")
        ,col("payload.after.source.lastRefreshedAt").alias("DataAssetLastUpdatedDatetime")
        ,col("payload.after.source.lastRefreshedBy").alias("DataAssetLastUpdatedByUserId")
        ,col("payload.after.systemData.createdAt").alias("CreatedDatetime")
        ,col("payload.after.systemData.createdBy").alias("CreatedByUserId")
        ,col("payload.after.systemData.lastModifiedAt").alias("ModifiedDateTime")
        ,col("payload.after.systemData.lastModifiedBy").alias("ModifiedByUserId")
        ,col("payload.after.domain").alias("BusinessDomainId"))
        .filter("operationType=='Create' or operationType=='Update'")

      val DeleteIsEmpty = df.filter("operationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        var dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.id").alias("DataAssetId")
          ,col("payload.before.type").alias("DataAssetType")
          ,col("payload.before.name").alias("AssetDisplayName")
          ,col("payload.after.description").alias("AssetDescription")
          ,col("payload.before.source.fqn").alias("FullyQualifiedName")
          ,col("payload.before.source.type").alias("ScanSource")
          ,col("payload.before.source.lastRefreshedAt").alias("DataAssetLastUpdatedDatetime")
          ,col("payload.before.source.lastRefreshedBy").alias("DataAssetLastUpdatedByUserId")
          ,col("payload.before.systemData.createdAt").alias("CreatedDatetime")
          ,col("payload.before.systemData.createdBy").alias("CreatedByUserId")
          ,col("payload.before.systemData.lastModifiedAt").alias("ModifiedDateTime")
          ,col("payload.before.systemData.lastModifiedBy").alias("ModifiedByUserId")
          ,col("payload.after.domain").alias("BusinessDomainId"))
          .filter("operationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      } else {
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter(s"""DataAssetId IS NOT NULL
                                      | AND AssetDisplayName IS NOT NULL
                                      | AND DataAssetType IS NOT NULL""".stripMargin).distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess,List("DataAssetType"),"DataAssetTypeId")
      dfProcess = dfProcess.withColumn("IsCertified",lit(null:IntegerType))

      dfProcess = dfProcess.select(col("DataAssetId").cast(StringType)
        ,col("DataAssetTypeId").cast(StringType)
        ,col("AssetDisplayName").alias("AssetDisplayName").cast(StringType)
        ,col("AssetDescription").alias("AssetDescription").cast(StringType)
        ,col("FullyQualifiedName").alias("FullyQualifiedName").cast(StringType)
        ,col("ScanSource").alias("ScanSource").cast(StringType)
        ,col("IsCertified").alias("IsCertified").cast(IntegerType)
        ,col("DataAssetLastUpdatedDatetime").alias("DataAssetLastUpdatedDatetime").cast(TimestampType)
        ,col("DataAssetLastUpdatedByUserId").alias("DataAssetLastUpdatedByUserId").cast(StringType)
        ,col("AccountId").alias("AccountId").cast(StringType)
        ,col("CreatedDatetime").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedByUserId").alias("CreatedByUserId").cast(StringType)
        ,col("ModifiedDateTime").alias("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").alias("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").alias("EventProcessingTime").cast(LongType)
        ,col("OperationType").alias("OperationType").cast(StringType)
        ,col("BusinessDomainId").alias("BusinessDomainId").cast(StringType)
      )

      val windowSpec = Window.partitionBy("DataAssetId").orderBy(col("ModifiedDateTime").desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DataAssetId is null
                            | or DataAssetTypeId is null
                            | or AssetDisplayName is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAsset Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAsset Data: ${e.getMessage}")
        throw e
    }
  }
  def writeData(df:DataFrame,adlsTargetDirectory:String,refreshType:String,ReProcessingThresholdInMins:Int): Unit = {
    try {
      val EntityName = "DataAsset"
      val IsProcessingRequired = new DeltaTableProcessingCheck(adlsTargetDirectory: String)
      if (!IsProcessingRequired.isDeltaTableRefreshedWithinXMinutes(EntityName,ReProcessingThresholdInMins)){
      val dfWrite = df.drop("BusinessDomainId")
        .distinct()

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
            val dfTargetDeltaTable = DeltaTable.forPath(spark, adlsTargetDirectory.concat("/").concat(EntityName))
            val dfTarget = dfTargetDeltaTable.toDF
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
                """target.DataAssetId = source.DataAssetId""")
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
                expr("target.DataAssetId = source.DataAssetId")
              )
              .whenMatched
              .delete()
              .execute()
          }
          else if(!df.isEmpty && dfTarget.isEmpty){
            println("Delta Table DataAsset Is Empty At Lake For incremental merge. Performing Full Overwrite...")
            dfWrite.write
              .format("delta")
              .mode("overwrite")
              .save(adlsTargetDirectory.concat("/").concat(EntityName))
          }
        }
        else{
          println("Delta Table DataAsset Does not exist for incremental merge. Performing Full Overwrite...")
          dfWrite.write
            .format("delta")
            .mode("overwrite")
            .save(adlsTargetDirectory.concat("/").concat(EntityName))
        }
      }
    }}
    catch{
      case e: Exception =>
        println(s"Error Writing/Merging DataAsset data: ${e.getMessage}")
        logger.error(s"Error Writing/Merging DataAsset data: ${e.getMessage}")
        throw e
    }
  }
}
