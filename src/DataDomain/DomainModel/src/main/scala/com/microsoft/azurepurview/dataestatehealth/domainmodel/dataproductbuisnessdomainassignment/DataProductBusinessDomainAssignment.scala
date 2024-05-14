package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductbuisnessdomainassignment
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, lit, lower, max, row_number, trim, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}

class DataProductBusinessDomainAssignment (spark: SparkSession, logger:Logger) {

  def processDataProductBusinessDomainAssignment(dfDataProduct:DataFrame,schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Read dependent Tables BusinessDomain For BusinessDomainId
      val dfBusinessDomain=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/BusinessDomain"))

      //Start Processing DataProduct Delta Table
      var dfProcess = dfDataProduct.select(col("DataProductId")
        ,col("BusinessDomainId")
        ,col("OperationType")
        ,col("ExpiredFlagLastModifiedDatetime"))

      dfProcess = dfProcess
        .withColumn("AssignedByUserId", lit(null: StringType))
        .withColumn("AssignmentDateTime", lit(null: TimestampType))
        .withColumn("ActiveFlag", when(lower(trim(col("OperationType"))) === "delete", 0).otherwise(1))
        .withColumnRenamed("ExpiredFlagLastModifiedDatetime","ActiveFlagLastModifiedDateTime")

      dfProcess = dfProcess.filter(s"""DataProductId IS NOT NULL
                                      | AND BusinessDomainId IS NOT NULL""".stripMargin).distinct()

      var dfJoin = dfProcess
        .join(dfBusinessDomain, dfProcess("BusinessDomainId") === dfBusinessDomain("BusinessDomainId"))

      dfJoin = dfJoin.select(col("DataProductId").cast(StringType)
        ,dfBusinessDomain("BusinessDomainId").alias("BusinessDomainId").cast(StringType)
        ,col("AssignedByUserId").alias("AssignedByUserId").cast(StringType)
        ,col("AssignmentDateTime").alias("AssignmentDateTime").cast(TimestampType)
        ,dfProcess("ActiveFlag").alias("ActiveFlag").cast(IntegerType)
        ,dfProcess("ActiveFlagLastModifiedDateTime").alias("ActiveFlagLastModifiedDateTime").cast(TimestampType))

      val dfProcessed = spark.createDataFrame(dfJoin.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataProductBusinessDomainAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing DataProductBusinessDomainAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
