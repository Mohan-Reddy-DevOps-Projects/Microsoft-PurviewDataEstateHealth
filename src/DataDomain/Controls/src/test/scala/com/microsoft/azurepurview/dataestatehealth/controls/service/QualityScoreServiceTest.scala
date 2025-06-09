package com.microsoft.azurepurview.dataestatehealth.controls.service

import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.scalatest.BeforeAndAfterAll
import org.scalatest.funsuite.AnyFunSuite
import org.apache.spark.sql.functions.{col, explode}
import org.apache.spark.sql.types._
import com.microsoft.azurepurview.dataestatehealth.controls.model.ControlRule
import org.apache.log4j.Logger
import org.apache.spark.sql.AnalysisException

class QualityScoreServiceTest extends AnyFunSuite with BeforeAndAfterAll {
  
  private var spark: SparkSession = _
  private var testDF: DataFrame = _
  private var qualityService: QualityScoreService = _
  private val logger = Logger.getLogger(classOf[QualityScoreServiceTest])
  
  override def beforeAll(): Unit = {
    super.beforeAll()
    
    // Create a local SparkSession
    spark = SparkSession.builder()
      .appName("QualityScoreServiceTest")
      .master("local[2]")
      .getOrCreate()
    
    // Create the service to test
    qualityService = new QualityScoreService(spark, logger)
    
    // Define test data schema
    val schema = StructType(Seq(
      StructField("id", IntegerType, nullable = false),
      StructField("name", StringType, nullable = true),
      StructField("age", IntegerType, nullable = true),
      StructField("email", StringType, nullable = true),
      StructField("is_active", BooleanType, nullable = true)
    ))
    
    // Create test data with explicit Row objects
    val data = Seq(
      Row(1, "John Doe", 30, "john@example.com", true),
      Row(2, "Jane Smith", 25, "jane@example.com", true),
      Row(3, "Bob Johnson", 40, null, false),
      Row(4, null, 35, "bob@example.com", true),
      Row(5, "Alice Brown", null, "alice@example.com", null)
    )
    
    testDF = spark.createDataFrame(
      spark.sparkContext.parallelize(data),
      schema
    )
  }
  
  override def afterAll(): Unit = {
    if (spark != null) {
      spark.stop()
    }
    super.afterAll()
  }
  
  test("applyDetailedRules should apply rules correctly") {
    // Create test rules
    val rules = Seq(
      ControlRule("rule1", "Rule 1", "Age check", "age > 30"),
      ControlRule("rule2", "Rule 2", "Name check", "name IS NOT NULL"),
      ControlRule("rule3", "Rule 3", "Activity check", "is_active = true")
    )
    
    // Apply the rules
    val result = qualityService.applyDetailedRules(testDF, rules)
    
    // Verify the result structure
    assert(result.columns.contains("rule_results"))
    
    // Verify the count of rows is unchanged
    assert(result.count() === testDF.count())
    
    // Verify some specific rules
    val ageRuleResults = result.select(
      explode(col("rule_results")).as("rule")
    ).filter(col("rule.key") === "rule1")
      .select(col("rule.value"))
    
    // Count how many pass the age rule (age > 30)
    val passCount = ageRuleResults.filter(col("value") === "PASS").count()
    assert(passCount === 2) // Only Bob and null_name have age > 30
    
    // Count how many fail the age rule
    val failCount = ageRuleResults.filter(col("value") === "FAIL").count()
    assert(failCount === 3) // John, Jane, and Alice (null age) have age <= 30 or null age
  }
  
  test("applyDetailedRules should handle null input") {
    // Test with null DataFrame
    val exception = intercept[RuntimeException] {
      qualityService.applyDetailedRules(null, Seq())
    }
    
    assert(exception.getMessage.contains("Input DataFrame is null"))
  }
  
  test("applyDetailedRules should handle empty rules") {
    // Test with empty rules
    val result = qualityService.applyDetailedRules(testDF, Seq())
    
    // Compare schemas and count instead of direct DataFrame comparison
    assert(result.schema.equals(testDF.schema))
    assert(result.count() === testDF.count())
    
    // Compare sample data to verify it's the same DataFrame
    val resultArray = result.collect()
    val testDFArray = testDF.collect()
    assert(resultArray.sameElements(testDFArray))
  }
  
  test("applyDetailedRules should handle null rules") {
    // Test with null rules
    val result = qualityService.applyDetailedRules(testDF, null)
    
    // Compare schemas and count instead of direct DataFrame comparison
    assert(result.schema.equals(testDF.schema))
    assert(result.count() === testDF.count())
    
    // Compare sample data to verify it's the same DataFrame
    val resultArray = result.collect()
    val testDFArray = testDF.collect()
    assert(resultArray.sameElements(testDFArray))
  }
  
  test("applyDetailedRules should handle invalid rule conditions") {
    // Create test rules with one invalid rule
    val rules = Seq(
      ControlRule("rule1", "Rule 1", "Age check", "age > 30"),
      ControlRule("rule2", "Rule 2", "Invalid check", "non_existent_column > 10"),
      ControlRule("rule3", "Rule 3", "Activity check", "is_active = true")
    )
    
    // This should throw an exception because of the invalid rule
    // Accept either a RuntimeException or an AnalysisException
    val exception = intercept[Exception] {
      qualityService.applyDetailedRules(testDF, rules)
    }
    
    // Either exception type should have an error message about the invalid column
    assert(exception.getMessage.contains("non_existent_column") || 
           exception.getMessage.contains("Failed to parse rule condition"))
  }
  
  test("applyDetailedRules should handle empty rule conditions") {
    // Create test rules with one empty condition
    val rules = Seq(
      ControlRule("rule1", "Rule 1", "Age check", "age > 30"),
      ControlRule("rule2", "Rule 2", "Empty check", ""),
      ControlRule("rule3", "Rule 3", "Activity check", "is_active = true")
    )
    
    // Apply the rules - empty condition should be skipped
    val result = qualityService.applyDetailedRules(testDF, rules)
    
    // Check that we have only rule1 and rule3 in the results
    val ruleKeys = result.select(
      explode(col("rule_results")).as("rule")
    ).select(col("rule.key")).distinct().collect().map(_.getString(0))
    
    assert(ruleKeys.contains("rule1"))
    assert(ruleKeys.contains("rule3"))
    assert(!ruleKeys.contains("rule2"))
  }
  
  test("applyDetailedRules should handle complex rule conditions with boolean logic") {
    // Create test rules with complex conditions
    val rules = Seq(
      ControlRule("rule1", "Rule 1", "Complex check 1", "age > 30 AND name IS NOT NULL"),
      ControlRule("rule2", "Rule 2", "Complex check 2", "age <= 30 OR is_active = false"),
      ControlRule("rule3", "Rule 3", "Complex check 3", "email IS NOT NULL AND (age > 30 OR name IS NULL)")
    )
    
    // Apply the rules
    val result = qualityService.applyDetailedRules(testDF, rules)
    
    // Verify results for complex rule1
    val rule1Results = result.select(
      explode(col("rule_results")).as("rule")
    ).filter(col("rule.key") === "rule1")
      .select(col("rule.value"))
    
    val rule1PassCount = rule1Results.filter(col("value") === "PASS").count()
    assert(rule1PassCount === 1) // Only Bob has age > 30 AND non-null name
    
    // Verify results for complex rule3
    val rule3Results = result.select(
      explode(col("rule_results")).as("rule")
    ).filter(col("rule.key") === "rule3")
      .select(col("rule.value"))
    
    val rule3PassCount = rule3Results.filter(col("value") === "PASS").count()
    assert(rule3PassCount === 1) // Only null_name has non-null email AND (age > 30 OR null name)
  }

  test("transformRuleResultsToFlattenedJson should transform array format to flattened JSON") {
    // Create test rules
    val rules = Seq(
      ControlRule("AlwaysFail", "Always Fail Rule", "Always fail test", "false"),
      ControlRule("dfc18455-36ea-4641-ab14-f812b64db9db", "GUID Rule", "GUID test", "age > 50")
    )
    
    // Apply the rules to get array format
    val resultWithArray = qualityService.applyDetailedRules(testDF, rules)
    
    // Transform to flattened JSON format
    val resultWithFlattenedJson = qualityService.transformRuleResultsToFlattenedJson(resultWithArray)
    
    // Verify the transformation
    assert(resultWithFlattenedJson.columns.contains("rule_results"))
    
    // Check that rule_results is now a string column with JSON format
    val jsonResults = resultWithFlattenedJson.select("rule_results").collect()
    
    // Verify each row has the expected flattened JSON format
    jsonResults.foreach { row =>
      val jsonString = row.getString(0)
      assert(jsonString.startsWith("{"))
      assert(jsonString.endsWith("}"))
      assert(jsonString.contains("\"AlwaysFail\":\"FAIL\""))
      assert(jsonString.contains("\"dfc18455-36ea-4641-ab14-f812b64db9db\":\"FAIL\""))
      // Verify it's properly formatted JSON with quotes around keys and values
      assert(jsonString.matches(".*\"[^\"]+\":\"[^\"]+\".*"))
    }
  }

  test("transformRuleResultsToFlattenedJson should handle DataFrame without rule_results column") {
    // Test with DataFrame that doesn't have rule_results column
    val result = qualityService.transformRuleResultsToFlattenedJson(testDF)
    
    // Should return the original DataFrame unchanged
    assert(result.schema.equals(testDF.schema))
    assert(result.count() === testDF.count())
  }

  test("transformRuleResultsToFlattenedJson should handle empty rule_results array") {
    // Create a DataFrame with empty rule_results array
    import org.apache.spark.sql.functions._
    val dfWithEmptyResults = testDF.withColumn("rule_results", array())
    
    // Transform to flattened JSON format
    val result = qualityService.transformRuleResultsToFlattenedJson(dfWithEmptyResults)
    
    // Verify it returns empty JSON object
    val jsonResults = result.select("rule_results").collect()
    jsonResults.foreach { row =>
      val jsonString = row.getString(0)
      assert(jsonString === "{}")
    }
  }
} 