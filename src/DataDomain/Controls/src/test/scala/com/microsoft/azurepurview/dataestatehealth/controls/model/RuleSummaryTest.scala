package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.scalatest.funsuite.AnyFunSuite

class RuleSummaryTest extends AnyFunSuite {

  test("RuleSummary should be created with explicitly provided values") {
    // Given
    val ruleId = "rule-001"
    val ruleName = "Test Rule"
    val description = "This is a test rule"
    val condition = "count(*) > 0"
    val totalCount = 100L
    val passedCount = 95L
    val failedCount = 5L
    val complianceScore = 95.0
    val executionTimeMs = 1200L
    val error = Some("Test error message")
    
    // When
    val summary = RuleSummary(
      ruleId = ruleId,
      ruleName = ruleName,
      description = description,
      condition = condition,
      totalCount = totalCount,
      passedCount = passedCount,
      failedCount = failedCount,
      complianceScore = complianceScore,
      executionTimeMs = executionTimeMs,
      error = error
    )
    
    // Then
    assert(summary.ruleId === ruleId)
    assert(summary.ruleName === ruleName)
    assert(summary.description === description)
    assert(summary.condition === condition)
    assert(summary.totalCount === totalCount)
    assert(summary.passedCount === passedCount)
    assert(summary.failedCount === failedCount)
    assert(summary.complianceScore === complianceScore)
    assert(summary.executionTimeMs === executionTimeMs)
    assert(summary.error === error)
  }
  
  test("RuleSummary should use default None for error when not provided") {
    // Given
    val ruleId = "rule-001"
    val ruleName = "Test Rule"
    val description = "This is a test rule"
    val condition = "count(*) > 0"
    val totalCount = 100L
    val passedCount = 95L
    val failedCount = 5L
    val complianceScore = 95.0
    val executionTimeMs = 1200L
    
    // When - create without specifying the error parameter
    val summary = RuleSummary(
      ruleId = ruleId,
      ruleName = ruleName,
      description = description,
      condition = condition,
      totalCount = totalCount,
      passedCount = passedCount,
      failedCount = failedCount,
      complianceScore = complianceScore,
      executionTimeMs = executionTimeMs
    )
    
    // Then - error should be None
    assert(summary.error === None)
  }
  
  test("RuleSummary instances should be comparable based on their contents") {
    // Given
    val summary1 = RuleSummary(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0",
      totalCount = 100L,
      passedCount = 95L,
      failedCount = 5L,
      complianceScore = 95.0,
      executionTimeMs = 1200L
    )
    
    val summary2 = RuleSummary(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0",
      totalCount = 100L,
      passedCount = 95L,
      failedCount = 5L,
      complianceScore = 95.0,
      executionTimeMs = 1200L
    )
    
    val differentSummary = RuleSummary(
      ruleId = "rule-002",
      ruleName = "Different Rule",
      description = "This is a different rule",
      condition = "count(*) > 10",
      totalCount = 200L,
      passedCount = 180L,
      failedCount = 20L,
      complianceScore = 90.0,
      executionTimeMs = 1500L
    )
    
    // Then
    assert(summary1 === summary2) // Should be equal based on content
    assert(summary1 !== differentSummary) // Should not be equal
  }
  
  test("RuleSummary should handle extreme values correctly") {
    // Given
    val zeroCounts = RuleSummary(
      ruleId = "rule-zero",
      ruleName = "Zero Rule",
      description = "Rule with zero counts",
      condition = "true",
      totalCount = 0L,
      passedCount = 0L,
      failedCount = 0L,
      complianceScore = 0.0,
      executionTimeMs = 0L
    )
    
    val maxValues = RuleSummary(
      ruleId = "rule-max",
      ruleName = "Max Values Rule",
      description = "Rule with maximum values",
      condition = "true",
      totalCount = Long.MaxValue,
      passedCount = Long.MaxValue,
      failedCount = 0L,
      complianceScore = 100.0,
      executionTimeMs = Long.MaxValue
    )
    
    // Then
    assert(zeroCounts.totalCount === 0L)
    assert(zeroCounts.complianceScore === 0.0)
    assert(maxValues.totalCount === Long.MaxValue)
    assert(maxValues.complianceScore === 100.0)
  }
  
  test("RuleSummary should handle long error messages") {
    // Given
    val longErrorMsg = "E" * 1000 // A very long error message with 1000 'E' characters
    
    // When
    val summary = RuleSummary(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0",
      totalCount = 100L,
      passedCount = 95L,
      failedCount = 5L,
      complianceScore = 95.0,
      executionTimeMs = 1200L,
      error = Some(longErrorMsg)
    )
    
    // Then
    assert(summary.error.isDefined)
    assert(summary.error.get.length === 1000)
  }
  
  test("RuleSummary toString should contain essential information") {
    // Given
    val summary = RuleSummary(
      ruleId = "rule-001",
      ruleName = "Test Rule",
      description = "This is a test rule",
      condition = "count(*) > 0",
      totalCount = 100L,
      passedCount = 95L,
      failedCount = 5L,
      complianceScore = 95.0,
      executionTimeMs = 1200L,
      error = Some("Test error")
    )
    
    // When
    val summaryString = summary.toString
    
    // Then
    assert(summaryString.contains("rule-001"))
    assert(summaryString.contains("Test Rule"))
    assert(summaryString.contains("This is a test rule"))
    assert(summaryString.contains("count(*) > 0"))
    assert(summaryString.contains("100"))
    assert(summaryString.contains("95"))
    assert(summaryString.contains("5"))
    assert(summaryString.contains("95.0"))
    assert(summaryString.contains("1200"))
    assert(summaryString.contains("Test error"))
  }
} 