package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.scalatest.funsuite.AnyFunSuite

class ControlRuleTest extends AnyFunSuite {

  test("ControlRule should be created with explicitly provided values") {
    // Given
    val ruleId = "rule-001"
    val ruleName = "Test Rule"
    val description = "This is a test rule"
    val condition = "count(*) > 0"
    val ruleType = "DataQuality"
    val tags = Map("category" -> "data_quality", "priority" -> "high")
    
    // When
    val rule = ControlRule(
      ruleId = ruleId,
      ruleName = ruleName,
      description = description,
      condition = condition,
      ruleType = ruleType,
      tags = tags
    )
    
    // Then
    assert(rule.ruleId === ruleId)
    assert(rule.ruleName === ruleName)
    assert(rule.description === description)
    assert(rule.condition === condition)
    assert(rule.ruleType === ruleType)
    assert(rule.tags === tags)
  }
  
  test("ControlRule should use default values when optional parameters are not provided") {
    // Given
    val ruleId = "rule-001"
    val ruleName = "Test Rule"
    val description = "This is a test rule"
    val condition = "count(*) > 0"
    
    // When - create the rule without specifying optional parameters
    val rule = ControlRule(
      ruleId = ruleId,
      ruleName = ruleName,
      description = description,
      condition = condition
    )
    
    // Then - default values should be used
    assert(rule.ruleId === ruleId)
    assert(rule.ruleName === ruleName)
    assert(rule.description === description)
    assert(rule.condition === condition)
    assert(rule.ruleType === "CustomTruth") // Default value for ruleType
    assert(rule.tags === Map.empty) // Default empty map for tags
  }
  
  test("ControlRule instances should be comparable based on their contents") {
    // Given
    val rule1 = ControlRule(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0"
    )
    
    val rule2 = ControlRule(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0"
    )
    
    val differentRule = ControlRule(
      ruleId = "rule-002",
      ruleName = "Different Rule",
      description = "This is a different rule",
      condition = "count(*) > 10"
    )
    
    // Then
    assert(rule1 === rule2) // Should be equal based on content
    assert(rule1 !== differentRule) // Should not be equal
  }
  
  test("ControlRule should allow creation with complex SQL conditions") {
    // Given
    val complexCondition = """
      |SELECT 
      |  COUNT(*) as row_count,
      |  SUM(CASE WHEN col1 IS NULL THEN 1 ELSE 0 END) as null_count
      |FROM my_table
      |HAVING null_count / row_count < 0.05
    """.stripMargin
    
    // When
    val rule = ControlRule(
      ruleId = "complex-rule",
      ruleName = "Complex SQL Rule",
      description = "Tests a complex SQL condition",
      condition = complexCondition
    )
    
    // Then
    assert(rule.condition === complexCondition)
    assert(rule.ruleType === "CustomTruth") // Default is still used
  }
  
  test("ControlRule toString should contain essential information") {
    // Given
    val rule = ControlRule(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0",
      ruleType = "DataQuality",
      tags = Map("category" -> "data_quality")
    )
    
    // When
    val ruleString = rule.toString
    
    // Then
    assert(ruleString.contains("rule-001"))
    assert(ruleString.contains("Test Rule"))
    assert(ruleString.contains("This is a test rule"))
    assert(ruleString.contains("count(*) > 0"))
    assert(ruleString.contains("DataQuality"))
    assert(ruleString.contains("category"))
    assert(ruleString.contains("data_quality"))
  }
} 