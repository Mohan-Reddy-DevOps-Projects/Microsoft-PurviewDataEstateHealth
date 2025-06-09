package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.scalatest.funsuite.AnyFunSuite

class CommandLineParserTest extends AnyFunSuite {

  test("parseArgs should correctly parse all command line arguments") {
    // Given command line arguments for all parameters
    val args = Array(
      "--AdlsTargetDirectory", "adls://container/path",
      "--ReProcessingThresholdInMins", "60",
      "--AccountId", "test-account-123",
      "--RefreshType", "Incremental",
      "--JobRunGuid", "job-123-guid",
      "--TenantId", "tenant-456"
    )

    // When parseArgs is called
    val config = CommandLineParser.parseArgs(args)

    // Then all values should be parsed correctly
    assert(config.adlsTargetDirectory === "adls://container/path")
    assert(config.reProcessingThresholdInMins === 60)
    assert(config.accountId === "test-account-123")
    assert(config.refreshType === "Incremental")
    assert(config.jobRunGuid === "job-123-guid")
    assert(config.tenantId === "tenant-456")
  }

  test("parseArgs should set default values for missing optional arguments") {
    // Given minimal command line arguments
    val args = Array(
      "--AdlsTargetDirectory", "adls://container/path",
      "--AccountId", "test-account-123",
      "--JobRunGuid", "job-123-guid"
    )

    // When parseArgs is called
    val config = CommandLineParser.parseArgs(args)

    // Then required values should be parsed and optional values should use defaults
    assert(config.adlsTargetDirectory === "adls://container/path")
    assert(config.reProcessingThresholdInMins === 0) // default value
    assert(config.accountId === "test-account-123")
    assert(config.refreshType === "Full") // default value
    assert(config.jobRunGuid === "job-123-guid")
    assert(config.tenantId === "") // default value
  }

  test("parseArgs should handle argument values with spaces") {
    // Given command line arguments with values containing spaces
    val args = Array(
      "--AdlsTargetDirectory", "adls://container/path with spaces",
      "--AccountId", "test account 123",
      "--JobRunGuid", "job 123 guid"
    )

    // When parseArgs is called
    val config = CommandLineParser.parseArgs(args)

    // Then values with spaces should be parsed correctly
    assert(config.adlsTargetDirectory === "adls://container/path with spaces")
    assert(config.accountId === "test account 123")
    assert(config.jobRunGuid === "job 123 guid")
  }

  test("parseArgs should throw IllegalArgumentException for invalid arguments") {
    // Given invalid command line arguments - using a malformed option that should cause a parsing error
    val args = Array(
      "--InvalidArg", "value" // This should fail because it's not a valid option
    )

    // When parseArgs is called with invalid arguments, Then it should throw IllegalArgumentException
    val exception = intercept[IllegalArgumentException] {
      CommandLineParser.parseArgs(args)
    }

    // Verify the exception message
    assert(exception.getMessage.contains("Failed to parse command line arguments"))
  }
  
  test("parseArgs should handle empty arguments array") {
    // Given empty command line arguments
    val args = Array.empty[String]

    // When parseArgs is called with empty arguments, Then it should throw IllegalArgumentException
    val exception = intercept[IllegalArgumentException] {
      CommandLineParser.parseArgs(args)
    }

    // Verify the exception message
    assert(exception.getMessage.contains("Failed to parse command line arguments"))
    assert(exception.getMessage.contains("empty arguments array"))
  }
  
  test("parseArgs should parse arguments in any order") {
    // Given command line arguments in a different order
    val args = Array(
      "--JobRunGuid", "job-123-guid",
      "--AdlsTargetDirectory", "adls://container/path",
      "--AccountId", "test-account-123"
    )

    // When parseArgs is called
    val config = CommandLineParser.parseArgs(args)

    // Then values should be parsed correctly regardless of order
    assert(config.adlsTargetDirectory === "adls://container/path")
    assert(config.accountId === "test-account-123")
    assert(config.jobRunGuid === "job-123-guid")
  }
} 