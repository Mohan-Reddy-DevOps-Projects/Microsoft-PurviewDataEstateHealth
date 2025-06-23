package com.microsoft.azurepurview.dataestatehealth.commonutils.writer

import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.max
import org.apache.spark.sql.{DataFrame, Row, SparkSession}

import java.sql.Timestamp
import java.text.SimpleDateFormat
import java.time.Instant
import java.util.UUID

class Maintenance (spark: SparkSession, logger:Logger) {
  def processDeltaTable(deltaTablePath: String): Unit = {
    // Check if the Delta table path exists
    if (deltaTableExists(deltaTablePath)) {
      // Perform optimize operation
      optimizeDeltaTable(deltaTablePath)
      // Perform vacuum operation
      vacuumDeltaTable(deltaTablePath)
    } else {
      println(s"Delta table does not exist at path: $deltaTablePath")
    }
  }

  private def deltaTableExists(deltaTablePath: String): Boolean = {
    try {
      DeltaTable.isDeltaTable(deltaTablePath)
    } catch {
      case _: Throwable => false
    }
  }

  private def vacuumDeltaTable(deltaTablePath: String): Unit = {
    try {
      DeltaTable.forPath(deltaTablePath).vacuum(168)
      println(s"Vacuum operation completed for Delta table at path: $deltaTablePath")
    } catch {
      case e: Exception =>
        println(s"Error during vacuum operation: ${e.getMessage}")
    }
  }

  private def optimizeDeltaTable(deltaTablePath: String): Unit = {
    try {
      DeltaTable.forPath(deltaTablePath).optimize().executeCompaction()
      println(s"Optimize operation completed for Delta table at path: $deltaTablePath")
    } catch {
      case e: Exception =>
        println(s"Error during optimize operation: ${e.getMessage}")
    }
  }
  def posixToTimestamp(posixTime: Long): String = {
    val sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss.SSS")
    sdf.format(new Timestamp(posixTime * 1000L))
  }
  def checkpointSentinel (accountId:String,deltaTablePath:String,df:Option[DataFrame],JobRunGuid:String,Entity:String,ExceptionStackTrace:String): Unit={
    // Exit early if switch to new controls flow is enabled
    val switchToNewControlsFlow = spark.conf.get("spark.ec.switchToNewControlsFlow", "false").toBoolean
    if (switchToNewControlsFlow) {
      return
    }
    
    try {
     val sentinelSchema = new SentinelSchema().sentinelSchema
     val maxEventProcessingTime = df match {
       case Some(dfNotNull) if dfNotNull.columns.contains("EventProcessingTime") =>
         dfNotNull.agg(max("EventProcessingTime")).collect()(0)(0).asInstanceOf[Long]
       case _ =>
         -1L
     }
     val maxEventProcessingTimestamp: String = if (maxEventProcessingTime != -1L) posixToTimestamp(maxEventProcessingTime) else null
     val dfRowCount = df match {
       case Some(dfNotNull) if !dfNotNull.isEmpty => dfNotNull.count()
       case _ =>
         0L
     }
     val Status = df match {
       case Some(dfNotNull) if (1==1)=>"Successful"
       case _ =>
         "Failed"
     }

     val data = Seq(
       Row(
         UUID.randomUUID().toString
         , JobRunGuid
         ,"DomainModel"
         , accountId
         , deltaTablePath
         , Entity
         ,dfRowCount
         ,Status
         ,Timestamp.from(Instant.now()).toString
         ,maxEventProcessingTime
         ,maxEventProcessingTimestamp
         ,ExceptionStackTrace
         ))
     val dfSentinel = spark.createDataFrame(spark.sparkContext.parallelize(data),schema=sentinelSchema)
     val reader = new Reader(spark,logger)
     //reader.writeCosmosData(dfSentinel,Entity)
     reader.writeToLogAnalyticsAsync(dfSentinel,Entity,"DEH_Spark_Logs")
     println(s"checkpointSentinel operation completed for Entity $Entity Delta table at path: $deltaTablePath")
   } catch {
     case e: Exception =>
       println(s"Error during checkpointSentinel operation for Entity $Entity Delta table at path: $deltaTablePath Error: ${e.getMessage}")
   }
 }
}
