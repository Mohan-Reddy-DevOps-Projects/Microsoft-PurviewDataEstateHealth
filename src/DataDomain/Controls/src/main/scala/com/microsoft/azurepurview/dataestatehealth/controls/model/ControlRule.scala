package com.microsoft.azurepurview.dataestatehealth.controls.model

/**
 * Represents a data quality control rule that can be applied to a dataset.
 *
 * @param ruleId Unique identifier for the rule
 * @param ruleName Human-readable name of the rule
 * @param description Description of what the rule checks for
 * @param condition SQL-like condition to evaluate (compatible with Spark SQL expressions)
 * @param ruleType Type of the rule (e.g., "CustomTruth")
 * @param tags Optional metadata tags associated with the rule
 */
case class ControlRule(
  ruleId: String,
  ruleName: String,
  description: String,
  condition: String,
  ruleType: String = "CustomTruth", // Default value if not provided
  tags: Map[String, String] = Map.empty
) 