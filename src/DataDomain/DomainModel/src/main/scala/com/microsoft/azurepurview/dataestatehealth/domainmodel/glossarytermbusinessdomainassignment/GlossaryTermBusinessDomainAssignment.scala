package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossarytermbusinessdomainassignment
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, lit, lower, max, row_number, trim, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}

class GlossaryTermBusinessDomainAssignment (spark: SparkSession, logger:Logger){
  def processGlossaryTermBusinessDomainAssignment(dfGlossaryTerm:DataFrame,schema: org.apache.spark.sql.types.StructType,adlsTargetDirectory:String):DataFrame={
    try{
      //Read dependent Tables BusinessDomain For BusinessDomainId
      val dfBusinessDomain=spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/BusinessDomain"))

      //Start Processing GlossaryTermBusinessDomainAssignment Delta Table
      var dfProcess = dfGlossaryTerm.select(col("GlossaryTermId")
        ,col("BusinessDomainId")
        ,col("OperationType")
        ,col("Status").alias("GlossaryTermStatus")
        ,col("ModifiedDateTime")
        ,col("CreatedDatetime")
        ,col("CreatedByUserId")
        ,col("ModifiedByUserId")
        ,col("EventProcessingTime")
        ,col("OperationType")
      )

      dfProcess = dfProcess.filter(s"""GlossaryTermId IS NOT NULL
                                      | AND BusinessDomainId IS NOT NULL
                                      |  AND GlossaryTermStatus IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess
        .withColumn("AssignedByUserId", lit(null: StringType))
        .withColumn("AssignmentDateTime", lit(null: TimestampType))
        .withColumn("ActiveFlag", when(lower(trim(col("OperationType"))) === "delete", 0).otherwise(1))
        .withColumn("ActiveFlagLastModifiedDatetime", col("ModifiedDateTime"))

      var dfJoin = dfProcess
        .join(dfBusinessDomain, dfProcess("BusinessDomainId") === dfBusinessDomain("BusinessDomainId"))

      dfJoin = dfJoin.select(dfProcess("GlossaryTermId").cast(StringType)
        ,dfBusinessDomain("BusinessDomainId").cast(StringType)
        ,dfProcess("AssignedByUserId").cast(StringType)
        ,dfProcess("AssignmentDateTime").cast(TimestampType)
        ,dfProcess("GlossaryTermStatus").cast(StringType)
        ,dfProcess("ActiveFlag").cast(IntegerType)
        ,dfProcess("ActiveFlagLastModifiedDatetime").cast(TimestampType)
        ,dfProcess("CreatedDatetime").cast(TimestampType)
        ,dfProcess("CreatedByUserId").cast(StringType)
        ,dfProcess("ModifiedDateTime").cast(TimestampType)
        ,dfProcess("ModifiedByUserId").cast(StringType)
        ,dfProcess("EventProcessingTime").cast(LongType)
        ,dfProcess("OperationType").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfJoin.rdd, schema=schema)

      val filterString = s"""GlossaryTermId is null
                            | or BusinessDomainId is null
                            | or GlossaryTermStatus is null""".stripMargin
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing GlossaryTermBusinessDomainAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing GlossaryTermBusinessDomainAssignment Data: ${e.getMessage}")
        throw e
    }
  }

}
