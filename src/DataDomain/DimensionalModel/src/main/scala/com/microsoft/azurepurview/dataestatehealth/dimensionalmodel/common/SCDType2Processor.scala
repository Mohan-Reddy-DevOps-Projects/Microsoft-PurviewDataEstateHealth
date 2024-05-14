package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.types.{IntegerType, LongType, TimestampType}

class SCDType2Processor (spark: SparkSession, logger:Logger){
  def processSCDType2Dimension(sourceData: DataFrame, targetDeltaPath: String, keyColumn: String, Entity:String, TargetSchema:org.apache.spark.sql.types.StructType): Unit = {
    try {
      val targetDeltaTableExists = DeltaTable.isDeltaTable(targetDeltaPath)

      if (!targetDeltaTableExists) {
        // If the target Delta table doesn't exist, add Id and other columns to the source DataFrame and write it to Target
        var updatedSourceData = sourceData
          .withColumn("EffectiveDateTime", current_timestamp())
          .withColumn("ExpiredDateTime", lit(null).cast(TimestampType))
          .withColumn("CurrentIndicator", lit(1).cast(IntegerType))

        val windowSpec = Window.partitionBy().orderBy(keyColumn)
        updatedSourceData = updatedSourceData.withColumn(Entity.concat("Id"), row_number().over(windowSpec))
        updatedSourceData = updatedSourceData.withColumn(Entity.concat("Id"), col(Entity.concat("Id")).cast(LongType))

        val reorderedColumns = (Entity.concat("Id") +: sourceData.columns.filter(_ != Entity.concat("Id"))) ++ Seq("EffectiveDateTime", "ExpiredDateTime", "CurrentIndicator")
        // Select the columns in the required order
        var reorderedSourceData = updatedSourceData.select(reorderedColumns.map(col): _*)

        // Write the updated source data to the target Delta path
        reorderedSourceData = spark.createDataFrame(reorderedSourceData.rdd, schema=TargetSchema)
        reorderedSourceData.write.format("delta").save(targetDeltaPath)
      }
      else {
      // Read the target Delta table
      val targetDeltaTable = DeltaTable.forPath(targetDeltaPath)
      val targetdf = targetDeltaTable.toDF


      val dfUtils = new DataFrameUtils(spark,logger)
      val maxId: Long = dfUtils.getMaxId(targetdf, Entity.concat("Id"))
        // Derive non-key columns from the source DataFrame
      sourceData.withColumnRenamed(Entity.concat("Id"),keyColumn)

      // STEP 1 - First of all Insert All New Records At Source and not in Target
      var insertDataDf = sourceData
        .withColumn("EffectiveDateTime", current_timestamp())
        .withColumn("ExpiredDateTime", lit(null).cast(TimestampType))
        .withColumn("CurrentIndicator", lit(1).cast(IntegerType))

      val windowSpecInsertNew = Window.partitionBy().orderBy(keyColumn)
      val joinCondition = s"source.$keyColumn = target.$keyColumn"
      val insertJoinDataDf = insertDataDf.alias("source")
      .join(targetdf.alias("target"),expr(joinCondition),"left_outer")
      .filter(col(s"target.$keyColumn").isNull)
      .selectExpr("source.*")

      //var finalInsertDf = insertJoinDataDf.select(insertDataDf.columns.map(col): _*)
      val finalInsertDf = insertJoinDataDf.withColumn(Entity.concat("Id"), row_number().over(windowSpecInsertNew)+maxId)

      val mergeCondition = s"target.$keyColumn = source.$keyColumn"

      targetDeltaTable.as("target")
        .merge(
          finalInsertDf.as("source"),
          mergeCondition)
        .whenNotMatched()
        .insertExpr(finalInsertDf.columns.map(col => col -> s"source.$col").toMap)
        .execute()

      // STEP 2 - NOW EXPIRE MATCHED
      //val allColumns = sourceData.columns.map(col => s"source.$col")
      val nonKeyColumns = sourceData.columns.filter(_ != keyColumn)
      val filterCondition = nonKeyColumns.map(col => s"target.$col <> source.$col").mkString(" OR ")

      val windowSpecMerge = Window.partitionBy(keyColumn).orderBy(desc("ModifiedDatetime"))
      val latestRecordDf = targetdf.withColumn("row_number", row_number.over(windowSpecMerge))
        .filter("row_number = 1")
        .drop("row_number")

      var latestExpireRecordDf = latestRecordDf.alias("source")
        .join(sourceData.alias("target"), expr(joinCondition), "inner")
        .filter(expr(filterCondition))
        .selectExpr("source.*")

      //Get The lastest record to Expire
        latestExpireRecordDf = latestExpireRecordDf
      .withColumn("ExpiredDateTime", current_timestamp())
      .withColumn("CurrentIndicator", lit(0))

      val expmergeCondition = s"target.${Entity.concat("Id")} = source.${Entity.concat("Id")}"
      targetDeltaTable.as("target")
        .merge(
          latestExpireRecordDf.as("source"),
          expr(expmergeCondition))
        .whenMatched()
        .updateExpr(Map(
          "ExpiredDateTime" -> "source.ExpiredDateTime",
          "CurrentIndicator" -> "source.CurrentIndicator"
        ))
        .execute()

      // STEP 3 - Now Insert new Records for existing SourceId and Activate them

      val filterConditionInsert = nonKeyColumns.map(col => s"target.$col <> source.$col").mkString(" OR ")
      val insertActivateDf = sourceData.alias("source")
        .join(latestRecordDf.alias("target"),expr(joinCondition),"inner")
        .filter(expr(filterConditionInsert))
        .selectExpr("source.*")
      //Get The lastest record to Activate
      var latestActivateRecordDf = insertActivateDf.withColumn("row_number", row_number.over(windowSpecMerge))
        .filter("row_number = 1")
        .drop("row_number")

      latestActivateRecordDf = latestActivateRecordDf
          .withColumn("EffectiveDateTime", current_timestamp())
          .withColumn("ExpiredDateTime", lit(null).cast(TimestampType))
          .withColumn("CurrentIndicator", lit(1).cast(IntegerType))

      val targetDeltaTable2 = DeltaTable.forPath(targetDeltaPath)
      val targetdf2 = targetDeltaTable.toDF

      val maxId2: Long = dfUtils.getMaxId(targetdf2, Entity.concat("Id"))
      latestActivateRecordDf = latestActivateRecordDf.withColumn(Entity.concat("Id"), row_number().over(windowSpecInsertNew)+maxId2)

      val mergeActivateCondition = s"target.${Entity.concat("Id")} = source.${Entity.concat("Id")}"
        targetDeltaTable2.as("target")
        .merge(
          latestActivateRecordDf.as("source"),
          mergeActivateCondition)
          .whenNotMatched()
          .insertExpr(latestActivateRecordDf.columns.map(col => col -> s"source.$col").toMap)
          .execute()
    }
    } catch {
      case e: Exception =>
        println(s"Error processing SCD Type 2 dimension: ${e.getMessage}")
        throw e
    }
  }
}
