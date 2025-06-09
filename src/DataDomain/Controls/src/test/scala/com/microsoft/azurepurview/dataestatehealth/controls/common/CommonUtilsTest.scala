package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.log4j.Logger
import org.mockito.ArgumentMatchers.{any, anyString}
import org.mockito.Mockito.{times, verify, when}
import org.scalatest.BeforeAndAfterEach
import org.scalatest.funsuite.AnyFunSuite
import org.scalatestplus.mockito.MockitoSugar

import scala.util.{Failure, Success}

class CommonUtilsTest extends AnyFunSuite with BeforeAndAfterEach with MockitoSugar {

  private var mockLogger: Logger = _

  override def beforeEach(): Unit = {
    super.beforeEach()
    mockLogger = mock[Logger]
  }

  test("safeExecute should return Some when operation succeeds") {
    // Given a successful operation
    val expectedResult = "success"
    val operation = "test operation"

    // When safeExecute is called
    val result = CommonUtils.safeExecute(operation, mockLogger) {
      expectedResult
    }

    // Then the result should be wrapped in Some
    assert(result.isDefined)
    assert(result.get === expectedResult)
  }

  test("safeExecute should throw RuntimeException when operation fails") {
    // Given a failing operation
    val operation = "test operation"
    val expectedException = new RuntimeException("Operation failed")

    // When safeExecute is called with a failing block, Then it should throw a RuntimeException
    val exception = intercept[RuntimeException] {
      CommonUtils.safeExecute(operation, mockLogger) {
        throw expectedException
      }
    }

    // Then the exception should be properly wrapped
    assert(exception.getMessage.contains("Failed to execute test operation"))
    assert(exception.getCause === expectedException)
  }

  test("safeExecuteWithTry should return Success when operation succeeds") {
    // Given a successful operation
    val expectedResult = "success"
    val operation = "test operation"

    // When safeExecuteWithTry is called
    val result = CommonUtils.safeExecuteWithTry(operation, mockLogger) {
      expectedResult
    }

    // Then the result should be a Success
    assert(result.isSuccess)
    assert(result === Success(expectedResult))
  }

  test("safeExecuteWithTry should throw RuntimeException when operation fails") {
    // Given a failing operation
    val operation = "test operation"
    val expectedException = new RuntimeException("Operation failed")

    // When safeExecuteWithTry is called with a failing block, Then it should throw a RuntimeException
    val exception = intercept[RuntimeException] {
      CommonUtils.safeExecuteWithTry(operation, mockLogger) {
        throw expectedException
      }
    }

    // Then the exception should be properly wrapped
    assert(exception.getMessage.contains("Failed to execute test operation"))
    assert(exception.getCause === expectedException)
  }

  test("validateStringParam should return true for non-empty strings") {
    // Given a valid non-empty string
    val validParam = "validValue"
    val paramName = "testParam"

    // When validateStringParam is called
    val result = CommonUtils.validateStringParam(validParam, paramName, mockLogger)

    // Then it should return true
    assert(result)
  }

  test("validateStringParam should return false for null") {
    // Given a null string
    val nullParam: String = null
    val paramName = "testParam"

    // When validateStringParam is called
    val result = CommonUtils.validateStringParam(nullParam, paramName, mockLogger)

    // Then it should return false and log an error
    assert(!result)
  }

  test("validateStringParam should return false for empty string") {
    // Given an empty string
    val emptyParam = ""
    val paramName = "testParam"

    // When validateStringParam is called
    val result = CommonUtils.validateStringParam(emptyParam, paramName, mockLogger)

    // Then it should return false and log an error
    assert(!result)
  }

  test("validateStringParam should return false for whitespace-only string") {
    // Given a whitespace-only string
    val whitespaceParam = "   "
    val paramName = "testParam"

    // When validateStringParam is called
    val result = CommonUtils.validateStringParam(whitespaceParam, paramName, mockLogger)

    // Then it should return false and log an error
    assert(!result)
  }

  test("constructPath should join non-empty components with forward slashes") {
    // Given path components
    val component1 = "part1"
    val component2 = "part2"
    val component3 = "part3"

    // When constructPath is called
    val result = CommonUtils.constructPath(component1, component2, component3)

    // Then it should join them with forward slashes
    assert(result === "part1/part2/part3")
  }

  test("constructPath should filter out empty components") {
    // Given path components with empty strings
    val component1 = "part1"
    val component2 = ""
    val component3 = "part3"

    // When constructPath is called
    val result = CommonUtils.constructPath(component1, component2, component3)

    // Then it should filter out empty components
    assert(result === "part1/part3")
  }

  test("constructPath should return empty string for empty components") {
    // Given empty path components
    val components = Array.empty[String]

    // When constructPath is called
    val result = CommonUtils.constructPath(components: _*)

    // Then it should return an empty string
    assert(result === "")
  }

  test("sanitizePathComponent should replace invalid characters with underscores") {
    // Given a path with invalid characters
    val unsafePath = "unsafe/path:with*special?chars"

    // When sanitizePathComponent is called
    val result = CommonUtils.sanitizePathComponent(unsafePath)

    // Then it should replace invalid characters with underscores
    assert(result === "unsafe_path_with_special_chars")
  }

  test("sanitizePathComponent should handle null input") {
    // Given a null path
    val nullPath: String = null

    // When sanitizePathComponent is called
    val result = CommonUtils.sanitizePathComponent(nullPath)

    // Then it should return an empty string
    assert(result === "")
  }

  test("sanitizePathComponent should not modify valid path components") {
    // Given a path with only valid characters
    val validPath = "valid-path_123.txt"

    // When sanitizePathComponent is called
    val result = CommonUtils.sanitizePathComponent(validPath)

    // Then it should not modify the path
    assert(result === validPath)
  }
} 