package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, _}
import org.apache.spark.sql.types.{BooleanType, LongType, StringType, TimestampType}

class DataAssetColumnClassificationAssignment (spark: SparkSession, logger:Logger) {
  def processDataAssetColumnClassificationAssignment(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("DataAssetId").cast(StringType)
        ,col("ColumnId").cast(StringType)
        ,col("ClassificationId").cast(StringType)
        ,col("ColumnClassification").cast(StringType)
        ,col("ModifiedDateTime").cast(TimestampType))
        .filter("OperationType=='Create' or OperationType=='Update'")
        .filter("ColumnClassification IS NOT NULL")
        .distinct()
        .drop("ColumnClassification")

      dfProcess = dfProcess.filter(s"""DataAssetId IS NOT NULL
                                      | AND ColumnId IS NOT NULL
                                      | AND ClassificationId IS NOT NULL""".stripMargin).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataAssetColumnClassificationAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing DataAssetColumnClassificationAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
