package com.microsoft.azurepurview.dataestatehealth.controls.model

/**
 * Summary of rule evaluation results.
 *
 * @param ruleId Unique identifier for the rule
 * @param ruleName Human-readable name of the rule
 * @param description Description of what the rule checks for
 * @param condition The condition that was evaluated
 * @param totalCount Total number of records evaluated
 * @param passedCount Number of records that passed the rule
 * @param failedCount Number of records that failed the rule
 * @param complianceScore Percentage of records that passed (0-100)
 * @param executionTimeMs Time taken to evaluate the rule in milliseconds
 * @param error Optional error message if rule evaluation failed
 */
case class RuleSummary(
  ruleId: String,
  ruleName: String,
  description: String,
  condition: String,
  totalCount: Long,
  passedCount: Long,
  failedCount: Long,
  complianceScore: Double,
  executionTimeMs: Long,
  error: Option[String] = None
) 