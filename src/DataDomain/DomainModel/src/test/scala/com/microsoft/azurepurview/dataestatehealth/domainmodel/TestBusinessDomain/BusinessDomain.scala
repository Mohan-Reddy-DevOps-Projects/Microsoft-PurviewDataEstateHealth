package com.microsoft.azurepurview.dataestatehealth.domainmodel.TestBusinessDomain
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.log4j.Logger
class BusinessDomain ( Spark:SparkSession, logger:Logger,targetPath: String ){
  def testBusinessDomainTable():String= {
    try{
      val df = Spark.read
        .format("delta")
        .load(targetPath.concat("/BusinessDomain"))
      val invalidRows = df.filter("BusinessDomainId is null or BusinessDomainName is null or AccountId is null")
      println("BusinessDomain Table Invalid Row Count = ".concat(invalidRows.count().toString))
      if (invalidRows.count()>0){
        logger.error(
          s"""Test Failure BusinessDomain Data:
             |'TEST BusinessDomainId is null or BusinessDomainName is null or AccountId is null'
             | has ${invalidRows.count().toString} records.""".stripMargin)
      }
      "BusinessDomain Table Test Complete!"
    }
    catch{
      case e: Exception =>
        println(s"Test Error Reading  BusinessDomain Table data: ${e.getMessage}")
        logger.error(s"Test Error Reading Test BusinessDomain Table data: ${e.getMessage}")
        throw e
    }}
}
