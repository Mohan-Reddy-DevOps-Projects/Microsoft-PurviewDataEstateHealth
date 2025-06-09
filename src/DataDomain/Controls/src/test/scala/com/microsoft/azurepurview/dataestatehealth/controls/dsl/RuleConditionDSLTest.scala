package com.microsoft.azurepurview.dataestatehealth.controls.dsl

import org.apache.spark.sql.{DataFrame, SparkSession, Row}
import org.scalatest.BeforeAndAfterAll
import org.scalatest.funsuite.AnyFunSuite
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types._
import com.microsoft.azurepurview.dataestatehealth.controls.dsl.RuleConditionDSL._
import scala.collection.JavaConverters._

class RuleConditionDSLTest extends AnyFunSuite with BeforeAndAfterAll {
  
  private var spark: SparkSession = _
  private var testDF: DataFrame = _
  
  override def beforeAll(): Unit = {
    super.beforeAll()
    
    // Create a local SparkSession
    spark = SparkSession.builder()
      .appName("RuleConditionDSLTest")
      .master("local[2]")
      .getOrCreate()
    
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
  
  // Tests for Direct Column References
  test("ColumnReference should be converted to Column") {
    val reference = ColumnReference("name")
    assert(reference.toColumn.toString() === "name")
  }
  
  test("DirectColumn should wrap a Spark column") {
    val direct = DirectColumn(col("age") > 30)
    val result = testDF.filter(direct.toColumn)
    // Bob (age=40), unnamed (age=35)
    assert(result.count() === 2)
  }
  
  test("Or operator should combine conditions") {
    val condition = Or(
      DirectColumn(col("age") > 35),
      DirectColumn(col("name").isNull)
    )
    
    val result = testDF.filter(condition.toColumn)
    // Bob with age=40 and the null name row (id 4)
    assert(result.count() === 2)
  }
  
  // Tests for rule parsing
  test("parse should handle simple column conditions") {
    val condition = "age > 30"
    val result = RuleConditionDSL.parse(condition, testDF)
    
    assert(result.isRight)
    val filteredDF = testDF.filter(result.right.get.toColumn)
    // Bob (age=40) and null_name (age=35)
    assert(filteredDF.count() === 2)
  }
  
  test("parse should handle null checks") {
    val condition = "name IS NOT NULL"
    val result = RuleConditionDSL.parse(condition, testDF)
    
    assert(result.isRight)
    val filteredDF = testDF.filter(result.right.get.toColumn)
    // John, Jane, Bob, Alice (all with non-null names)
    assert(filteredDF.count() === 4)
  }
  
  test("parse should handle complex conditions") {
    val condition = "age > 30 AND is_active = true"
    val result = RuleConditionDSL.parse(condition, testDF)
    
    assert(result.isRight)
    val filteredDF = testDF.filter(result.right.get.toColumn)
    // The unnamed person (id 4) has age=35 and is_active=true
    assert(filteredDF.count() === 1)
  }
  
  test("parse should handle JavaScript-style operators") {
    val condition = "age === 30 && is_active == true"
    val result = RuleConditionDSL.parse(condition, testDF)
    
    assert(result.isRight)
    val filteredDF = testDF.filter(result.right.get.toColumn)
    // Only John matches (age=30, is_active=true)
    assert(filteredDF.count() === 1)
  }
  
  // Tests for applyRule
  test("applyRule should filter data based on condition") {
    val condition = "age > 30 AND email IS NOT NULL"
    val result = RuleConditionDSL.applyRule(testDF, condition)
    
    // Bob (age=40, email=null) doesn't match 
    // unnamed_person (id 4) (age=35, email=bob@example.com) matches
    // Alice (age=null, email=alice@example.com) doesn't match due to null age
    assert(result.count() === 1)
  }
  
  test("applyRule should handle JS/SQL mixed syntax") {
    val condition = "age > 25 && email IS NOT NULL"
    
    val result = RuleConditionDSL.applyRule(testDF, condition)
    
    // John (age=30, email=non-null)
    // Jane (age=25, email=non-null) - not included due to age > 25
    // Bob (age=40, email=null) - not included because email is null
    // unnamed_person (id 4) (age=35, email=non-null)
    // Alice (age=null, email=non-null) - not included because age is null
    assert(result.count() === 2)
    
    // Verify it contains expected IDs
    val ids = result.select("id").collect().map(_.getInt(0)).toSet
    assert(ids === Set(1, 4))
  }
  
  // Tests for validateCondition
  test("validateCondition should return None for valid conditions") {
    val condition = "age > 30"
    val result = RuleConditionDSL.validateCondition(condition, testDF)
    
    assert(result.isEmpty)
  }
  
  test("DSL should handle double quoted strings") {
    val condition = "name = \"John Doe\""
    val result = RuleConditionDSL.applyRule(testDF, condition)
    
    assert(result.count() === 1) // Only John
  }
  
  test("DSL should handle regexReplace expressions") {
    // This test is checking if username part of email (before @) is longer than 3 chars
    val condition = "email IS NOT NULL AND length(regexp_replace(email, '@.*', '')) > 3"
    val result = RuleConditionDSL.applyRule(testDF, condition)
    
    // All rows with non-null emails where username length > 3
    // John (john@example.com) - "john" is 4 chars -> matches
    // Jane (jane@example.com) - "jane" is 4 chars -> matches
    // Bob (id=3) has null email -> doesn't match
    // Unnamed (id=4) (bob@example.com) - "bob" is 3 chars -> doesn't match
    // Alice (alice@example.com) - "alice" is 5 chars -> matches
    assert(result.count() === 3)
    
    // Verify the specific IDs that should match
    val ids = result.select("id").collect().map(_.getInt(0)).toSet
    assert(ids === Set(1, 2, 5))
  }
} 