package com.microsoft.azurepurview.dataestatehealth.controls.dsl

import org.apache.spark.sql.{Column, DataFrame}
import org.apache.spark.sql.functions.{col, expr, lit, not}
import org.slf4j.LoggerFactory

/**
 * Domain-specific language for expressing rule conditions in data quality evaluation.
 * Provides a type-safe and composable way to define rule conditions using Spark Column expressions.
 */
object RuleConditionDSL {
  private val logger = LoggerFactory.getLogger(getClass)
  
  /**
   * Base trait for rule conditions that can be converted to Spark Column expressions
   */
  sealed trait RuleCondition {
    def toColumn: Column
    
    def &&(other: RuleCondition): RuleCondition = And(this, other)
    def ||(other: RuleCondition): RuleCondition = Or(this, other)
    def unary_! : RuleCondition = Not(this)
  }
  
  /**
   * Direct column reference
   */
  case class ColumnReference(columnName: String) extends RuleCondition {
    def toColumn: Column = col(columnName)
  }
  
  /**
   * Wraps a Spark column expression
   */
  case class DirectColumn(column: Column) extends RuleCondition {
    def toColumn: Column = column
  }
  
  /**
   * Logical operators
   */
  case class And(left: RuleCondition, right: RuleCondition) extends RuleCondition {
    def toColumn: Column = left.toColumn && right.toColumn
  }
  
  case class Or(left: RuleCondition, right: RuleCondition) extends RuleCondition {
    def toColumn: Column = left.toColumn || right.toColumn
  }
  
  case class Not(condition: RuleCondition) extends RuleCondition {
    def toColumn: Column = not(condition.toColumn)
  }
  
  /**
   * Creates a RuleCondition from a string condition.
   * This is the main entry point for applying rules.
   *
   * @param df DataFrame to evaluate against
   * @param condition String condition to parse
   * @return DataFrame filtered by the condition
   */
  def applyRule(df: DataFrame, condition: String): DataFrame = {
    if (condition == null || condition.trim.isEmpty) {
      logger.info("Empty condition, returning original DataFrame")
      return df
    }
    
    parse(condition, df) match {
      case Right(ruleCondition) => 
        logger.info(s"Successfully parsed condition: $condition")
        df.filter(ruleCondition.toColumn)
      case Left(error) => 
        logger.error(s"Failed to parse condition: $condition - ${error.getMessage}")
        throw new RuntimeException(s"Failed to parse rule condition: $condition - ${error.getMessage}")
    }
  }
  
  /**
   * Parses a string condition into a RuleCondition
   * 
   * @param condition String condition to parse
   * @param df DataFrame to validate column existence
   * @return Either a RuleCondition or an error
   */
  def parse(condition: String, df: DataFrame): Either[Throwable, RuleCondition] = {
    if (condition == null || condition.trim.isEmpty) {
      return Left(new IllegalArgumentException("Empty condition"))
    }
    
    try {
      val preprocessed = preprocess(condition)
      logger.debug(s"Preprocessed condition: $preprocessed")
      
      // Convert to a Spark Column expression
      val column = expr(preprocessed)
      Right(DirectColumn(column))
    } catch {
      case e: Exception =>
        // Fall back to legacy parsing for specific patterns
        try {
          parseLegacy(condition, df) match {
            case Some(ruleCondition) => Right(ruleCondition)
            case None => Left(e)
          }
        } catch {
          case fallbackError: Exception =>
            logger.error(s"Error parsing condition '$condition': ${e.getMessage}. Fallback also failed: ${fallbackError.getMessage}")
            Left(e)
        }
    }
  }
  
  /**
   * Preprocesses a condition string to convert JavaScript/Java style syntax to SQL syntax
   */
  private def preprocess(condition: String): String = {
    val transformations = Seq(
      // Convert JavaScript-style operators to SQL
      ("""\s*(===|==)\s*""".r, " = "),
      ("""\s*!==\s*""".r, " <> "),
      ("""\s*!=\s*""".r, " <> "),
      ("""\s*&&\s*""".r, " AND "),
      ("""\s*\|\|\s*""".r, " OR "),
      
      // Convert null checks
      ("""notNull\s*\(\s*([^)]+)\s*\)""".r, "$1 IS NOT NULL"),
      ("""isNull\s*\(\s*([^)]+)\s*\)""".r, "$1 IS NULL"),
      
      // Handle string literals consistently
      ("""([^=><])(["'])(.+?)\2""".r, "$1'$3'"),
      
      // Handle special functions
      ("""length\s*\(\s*regexReplace\s*\(\s*([^,]+),\s*(['"])(.*?)\2,\s*(['"])(.*?)\4\s*\)\s*\)""".r, 
       "length(regexp_replace($1, '$3', '$5'))"),
      
      // Handle boolean literals
      ("""\btrue\b""".r, "TRUE"),
      ("""\bfalse\b""".r, "FALSE")
    )
    
    // Apply all transformations
    transformations.foldLeft(condition) { 
      case (updatedCondition, (pattern, replacement)) =>
        pattern.replaceAllIn(updatedCondition, replacement)
    }
  }
  
  /**
   * Legacy parser for specific patterns that might not work with the direct approach
   */
  private def parseLegacy(condition: String, df: DataFrame): Option[RuleCondition] = {
    // Handle simple column existence checks
    val columnNames = df.columns.toSet
    if (columnNames.contains(condition.trim)) {
      logger.info(s"Column existence check: $condition")
      return Some(DirectColumn(col(condition.trim).isNotNull && col(condition.trim) === true))
    }
    
    // Handle direct comparisons with column names
    val columnComparisonPattern = """([^=><\s]+)\s*(==|===|!=|!==|>|<|>=|<=)\s*(.+)""".r
    condition.trim match {
      case columnComparisonPattern(column, op, value) if columnNames.contains(column.trim) =>
        val normalizedOp = op match {
          case "==" | "===" => "="
          case "!=" | "!==" => "<>"
          case other => other
        }
        
        val sqlExpr = s"${column.trim} $normalizedOp ${value.trim}"
        logger.info(s"Column comparison: $sqlExpr")
        Some(DirectColumn(expr(sqlExpr)))
        
      case _ => None
    }
  }
  
  /**
   * Validates that a condition can be evaluated against a DataFrame
   */
  def validateCondition(condition: String, df: DataFrame): Option[String] = {
    if (condition == null || condition.trim.isEmpty) {
      return Some("Empty condition")
    }
    
    try {
      // Try parsing
      parse(condition, df) match {
        case Right(_) => None // No error
        case Left(e) => Some(e.getMessage)
      }
    } catch {
      case e: Exception => Some(e.getMessage)
    }
  }
} 