package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.log4j.Logger

import scala.util.{Failure, Success, Try}

/**
 * Shared utilities for the Controls package providing common functionality
 * for error handling, validation, and path construction.
 */
object CommonUtils {
  
  /**
   * Safely executes a function with proper error handling and returns an Option
   * If an error occurs, the exception will be bubbled up to fail the Spark job
   *
   * @param operation The operation description for logging
   * @param logger The logger to use
   * @param block The code block to execute safely
   * @tparam T The return type of the block
   * @return An option containing the result or None if an error occurred
   */
  def safeExecute[T](operation: String, logger: Logger)(block: => T): Option[T] = {
    Try(block) match {
      case Success(result) => 
        Some(result)
      case Failure(e) =>
        logger.error(s"Error executing $operation: ${e.getMessage}", e)
        throw new RuntimeException(s"Failed to execute $operation: ${e.getMessage}", e)
    }
  }
  
  /**
   * Safely executes a function with proper error handling and returns the Try result
   * If an error occurs, the exception will be bubbled up to fail the Spark job
   *
   * @param operation The operation description for logging
   * @param logger The logger to use
   * @param block The code block to execute safely
   * @tparam T The return type of the block
   * @return The Try containing the result (Success or Failure)
   */
  def safeExecuteWithTry[T](operation: String, logger: Logger)(block: => T): Try[T] = {
    val result = Try(block)
    result match {
      case Failure(e) =>
        logger.error(s"Error executing $operation: ${e.getMessage}", e)
        throw new RuntimeException(s"Failed to execute $operation: ${e.getMessage}", e)
      case _ => // Success case doesn't need special handling
    }
    result
  }
  
  /**
   * Validates that a string parameter is not null or empty
   *
   * @param value The string value to check
   * @param paramName The parameter name for error reporting
   * @param logger The logger to use
   * @return true if the value is valid, false otherwise
   */
  def validateStringParam(value: String, paramName: String, logger: Logger): Boolean = {
    if (value == null || value.trim.isEmpty) {
      logger.error(s"Parameter '$paramName' is null or empty")
      false
    } else {
      true
    }
  }
  
  /**
   * Constructs a path from components with proper handling of separators
   *
   * @param components The path components to join
   * @return A properly formatted path with forward slashes
   */
  def constructPath(components: String*): String = {
    components.filter(_.nonEmpty).mkString("/")
  }
  
  /**
   * Sanitizes a string for safe usage as a file or directory name
   *
   * @param input The string to sanitize
   * @return A sanitized string
   */
  def sanitizePathComponent(input: String): String = {
    if (input == null) return ""
    input.replaceAll("[^a-zA-Z0-9_.-]", "_")
  }
} 