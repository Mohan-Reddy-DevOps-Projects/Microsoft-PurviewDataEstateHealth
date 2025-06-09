package com.microsoft.azurepurview.dataestatehealth.controls.service

import org.apache.log4j.Logger
import org.apache.spark.sql.DataFrame
import org.apache.spark.sql.functions.{col, lit, struct, when, array, udf, to_json, map_from_arrays}
import com.microsoft.azurepurview.dataestatehealth.controls.constants.Constants
import com.microsoft.azurepurview.dataestatehealth.controls.dsl.RuleConditionDSL
import com.microsoft.azurepurview.dataestatehealth.controls.model.ControlRule
import org.apache.spark.sql.SparkSession
import org.apache.spark.sql.types.{StringType, StructType}
import scala.collection.mutable

/**
 * Handles evaluation of quality rules and application to datasets.
 * Provides functionality to apply rules to DataFrames while preserving the original data structure.
 *
 * @param spark The SparkSession instance
 * @param logger Optional logger instance (will create one if not provided)
 */
class QualityScoreService(spark: SparkSession, private val logger: Logger = Logger.getLogger(classOf[QualityScoreService])) {

  import Constants.Columns._
  import Constants.RuleStatus._
  
  /**
   * Applies rules to a DataFrame and returns the original DataFrame with an additional array column
   * containing rule results. This preserves all columns from the input DataFrame while adding rule evaluation results.
   *
   * @param df The input DataFrame to evaluate
   * @param rules The rules to apply
   * @return The original DataFrame with an additional 'rule_results' column containing an array of structs
   */
  def applyDetailedRules(df: DataFrame, rules: Seq[ControlRule]): DataFrame = {
    if (df == null) {
      throw new RuntimeException("Input DataFrame is null, cannot apply rules")
    }
    
    if (rules == null || rules.isEmpty) {
      logger.warn("No rules to apply, returning original DataFrame")
      return df
    }
    
    logger.info(s"Applying ${rules.size} rules to DataFrame and generating detailed results")
    
    import org.apache.spark.sql.functions.{array, when, lit, struct}
    
    // Create array element expressions for each rule
    val ruleResultsExpressions = rules.flatMap { rule =>
      val ruleId = rule.ruleId
      if (rule.condition.nonEmpty) {
        try {
          // Create a rule condition column
          val ruleConditionResult = RuleConditionDSL.parse(rule.condition, df) match {
            case Right(ruleCondition) => ruleCondition.toColumn
            case Left(error) => 
              logger.error(s"Failed to parse rule condition: ${error.getMessage}")
              throw new RuntimeException(s"Failed to parse rule condition: ${error.getMessage}")
          }
          
          // Create a struct expression for this rule
          Some(
            struct(
              lit(ruleId).as("key"),
              when(ruleConditionResult, lit("PASS")).otherwise(lit("FAIL")).as("value")
            )
          )
        } catch {
          case e: Exception =>
            logger.error(s"Error processing rule $ruleId: ${e.getMessage}", e)
            None
        }
      } else {
        None
      }
    }
    
    // Add a single column with array of rule results
    val dfWithRuleResults = if (ruleResultsExpressions.nonEmpty) {
      df.withColumn("rule_results", array(ruleResultsExpressions: _*)).cache()
    } else {
      logger.warn("No valid rule expressions created, returning original DataFrame")
      df
    }
    
    dfWithRuleResults
  }

  /**
   * Transforms the rule_results array column to a flattened JSON string format.
   * Converts from: [{"key":"rule1","value":"PASS"},{"key":"rule2","value":"FAIL"}]
   * To: {"rule1":"PASS","rule2":"FAIL"}
   *
   * @param df The DataFrame with rule_results column
   * @return DataFrame with the rule_results column transformed to flattened JSON format
   */
  def transformRuleResultsToFlattenedJson(df: DataFrame): DataFrame = {
    import org.apache.spark.sql.functions._
    import spark.implicits._
    
    // Check if rule_results column exists
    if (!df.columns.contains("rule_results")) {
      logger.warn("DataFrame does not contain 'rule_results' column, returning original DataFrame")
      return df
    }
    
    logger.info("Transforming rule_results array to flattened JSON format")
    
    // Define UDF to convert array of structs to JSON string
    val arrayToJsonUDF = udf((ruleResultsArray: Seq[org.apache.spark.sql.Row]) => {
      if (ruleResultsArray == null || ruleResultsArray.isEmpty) {
        "{}"
      } else {
        val jsonMap = mutable.Map[String, String]()
        ruleResultsArray.foreach { row =>
          if (row != null && row.length >= 2) {
            val key = Option(row.getString(0)).getOrElse("")
            val value = Option(row.getString(1)).getOrElse("")
            if (key.nonEmpty) {
              jsonMap(key) = value
            }
          }
        }
        // Convert to JSON manually to ensure proper format
        "{" + jsonMap.map { case (k, v) => s""""$k":"$v"""" }.mkString(",") + "}"
      }
    })
    
    // Transform the rule_results column
    df.withColumn("rule_results", arrayToJsonUDF(col("rule_results")))
  }
} 