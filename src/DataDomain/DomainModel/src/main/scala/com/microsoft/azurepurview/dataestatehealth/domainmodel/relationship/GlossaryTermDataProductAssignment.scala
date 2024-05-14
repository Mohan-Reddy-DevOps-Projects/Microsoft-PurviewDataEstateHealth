package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, lit, lower, trim, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}
class GlossaryTermDataProductAssignment (spark: SparkSession, logger:Logger){
  def processGlossaryTermBusinessDomainAssignment(schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Read dependent Table Relationship
      val df=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/Relationship"))

      //Start Processing GlossaryTermBusinessDomainAssignment Delta Table
      var dfProcess = df.select(col("SourceType")
        ,col("SourceId")
        ,col("TargetType")
        ,col("TargetId")
        ,col("ModifiedByUserId")
        ,col("ModifiedDateTime")
        ,col("EventProcessingTime")
        ,col("OperationType")
      ).filter("SourceType='Term' and TargetType='DataProduct'")

      var df2 = df.select(col("SourceType")
        ,col("SourceId")
        ,col("TargetType")
        ,col("TargetId")
        ,col("ModifiedByUserId")
        ,col("ModifiedDateTime")
        ,col("EventProcessingTime")
        ,col("OperationType")).filter("SourceType='DataProduct' and TargetType='Term'")

      df2 = df2.select(col("TargetType").alias("SourceType")
        ,col("TargetId").alias("SourceId")
        ,col("SourceType").alias("TargetType")
        ,col("SourceId").alias("TargetId")
        ,col("ModifiedByUserId")
        ,col("ModifiedDateTime")
        ,col("EventProcessingTime")
        ,col("OperationType"))

      dfProcess = dfProcess.union(df2).distinct()

      dfProcess = dfProcess
        .withColumn("GlossaryTermID", col("SourceId"))
        .withColumn("DataProductId", col("TargetId"))
        .withColumn("ActiveFlag", when(lower(trim(col("OperationType"))) === "delete", 0).otherwise(1))
        .withColumn("ActiveFlagLastModifiedDatetime", col("ModifiedDateTime"))

      dfProcess = dfProcess.select(col("GlossaryTermID").cast(StringType)
        ,col("DataProductId").cast(StringType)
        ,col("ActiveFlag").cast(IntegerType)
        ,col("ActiveFlagLastModifiedDatetime").cast(TimestampType)
        ,col("ModifiedDateTime").cast(TimestampType)
        ,col("ModifiedByUserId").cast(StringType)
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType").cast(StringType)
      )

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing GlossaryTermDataProductAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing GlossaryTermDataProductAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
