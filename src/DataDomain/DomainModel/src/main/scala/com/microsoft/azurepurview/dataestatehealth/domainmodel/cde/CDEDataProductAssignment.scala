package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class CDEDataProductAssignment(spark: SparkSession, logger: Logger) {

  def processCDEDataProductAssignment(adlsTargetDirectory: String, schema: org.apache.spark.sql.types.StructType): DataFrame = {
    try {

      val dfRelationship = spark.read.format("delta").load(adlsTargetDirectory.concat("/Relationship"))

      val dfProcess1 = dfRelationship.select(col("SourceType")
        , col("SourceId")
        , col("TargetType")
        , col("TargetId")
        , col("ModifiedByUserId")
        , col("ModifiedDateTime")
        , col("EventProcessingTime")
        , col("OperationType")
      ).filter("SourceType='CriticalDataElement' and TargetType='DataProduct'")

      val dfProcess2 = dfRelationship.select(col("TargetType")
        , col("TargetId")
        , col("SourceType")
        , col("SourceId")
        , col("ModifiedByUserId")
        , col("ModifiedDateTime")
        , col("EventProcessingTime")
        , col("OperationType")
      ).filter("TargetType='CriticalDataElement' and SourceType='DataProduct'")

      val dfProcess = dfProcess1.union(dfProcess2)
        .withColumn("CDEId", col("SourceId"))
        .withColumn("DataProductId", col("TargetId"))

      val windowSpec = Window.partitionBy("CDEId","DataProductId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)

      val dfLatest = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()
        .select(col("CDEId").cast(StringType).alias("CriticalDataElementId")
          ,col("DataProductId").cast(StringType)
          ,col("ModifiedDateTime").cast(TimestampType)
          ,col("ModifiedByUserId").cast(StringType)
          ,col("EventProcessingTime").cast(LongType)
          ,col("OperationType").cast(StringType)
        )

      val dfFiltered = dfLatest.filter(col("OperationType") =!= "Delete").drop("OperationType")

      val dfProcessed = spark.createDataFrame(dfFiltered.rdd, schema=schema)

      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing CDEDataProductAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing CDEDataProductAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
