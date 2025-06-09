package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.scalatest.funsuite.AnyFunSuite

class JobConfigTest extends AnyFunSuite {

  test("JobConfig constructor should create instance with provided values") {
    // Given
    val adlsTargetDirectory = "adls://container/path"
    val accountId = "test-account-123"
    val refreshType = "Incremental" 
    val jobRunGuid = "job-123-guid"
    val reProcessingThresholdInMins = 60
    
    // When
    val config = JobConfig(
      adlsTargetDirectory = adlsTargetDirectory,
      accountId = accountId,
      refreshType = refreshType,
      jobRunGuid = jobRunGuid,
      reProcessingThresholdInMins = reProcessingThresholdInMins
    )
    
    // Then
    assert(config.adlsTargetDirectory === adlsTargetDirectory)
    assert(config.accountId === accountId)
    assert(config.refreshType === refreshType)
    assert(config.jobRunGuid === jobRunGuid)
    assert(config.reProcessingThresholdInMins === reProcessingThresholdInMins)
  }

  test("JobConfig.fromArgs should create instance with all provided arguments") {
    // Given
    val args = Array(
      "adls://container/path",
      "test-account-123",
      "Incremental",
      "job-123-guid"
    )
    val reProcessingThresholdInMins = 60
    
    // When
    val config = JobConfig.fromArgs(args, reProcessingThresholdInMins)
    
    // Then
    assert(config.adlsTargetDirectory === "adls://container/path")
    assert(config.accountId === "test-account-123")
    assert(config.refreshType === "Incremental")
    assert(config.jobRunGuid === "job-123-guid")
    assert(config.reProcessingThresholdInMins === reProcessingThresholdInMins)
  }
  
  test("JobConfig.fromArgs should use default values for missing arguments") {
    // Given
    val args = Array("adls://container/path", "test-account-123")
    val reProcessingThresholdInMins = 60
    
    // When
    val config = JobConfig.fromArgs(args, reProcessingThresholdInMins)
    
    // Then
    assert(config.adlsTargetDirectory === "adls://container/path")
    assert(config.accountId === "test-account-123")
    assert(config.refreshType === "Complete") // Default value
    assert(config.jobRunGuid.nonEmpty) // Random UUID was generated
    assert(config.reProcessingThresholdInMins === reProcessingThresholdInMins)
  }
  
  test("JobConfig.fromArgs should handle empty arguments array") {
    // Given
    val args = Array.empty[String]
    val reProcessingThresholdInMins = 60
    
    // When
    val config = JobConfig.fromArgs(args, reProcessingThresholdInMins)
    
    // Then
    assert(config.adlsTargetDirectory === "")
    assert(config.accountId === "")
    assert(config.refreshType === "Complete")
    assert(config.jobRunGuid.nonEmpty) // Random UUID was generated
    assert(config.reProcessingThresholdInMins === reProcessingThresholdInMins)
  }
  
  test("JobConfig instances should be comparable based on their contents") {
    // Given
    val config1 = JobConfig(
      adlsTargetDirectory = "adls://container/path",
      accountId = "test-account-123",
      refreshType = "Incremental",
      jobRunGuid = "job-123-guid",
      reProcessingThresholdInMins = 60
    )
    
    val config2 = JobConfig(
      adlsTargetDirectory = "adls://container/path",
      accountId = "test-account-123",
      refreshType = "Incremental",
      jobRunGuid = "job-123-guid",
      reProcessingThresholdInMins = 60
    )
    
    val differentConfig = JobConfig(
      adlsTargetDirectory = "adls://different/path",
      accountId = "different-account",
      refreshType = "Complete",
      jobRunGuid = "different-guid",
      reProcessingThresholdInMins = 30
    )
    
    // Then
    assert(config1 === config2) // Should be equal based on content
    assert(config1 !== differentConfig) // Should not be equal
  }
  
  test("JobConfig toString should contain essential information") {
    // Given
    val config = JobConfig(
      adlsTargetDirectory = "adls://container/path",
      accountId = "test-account-123",
      refreshType = "Incremental",
      jobRunGuid = "job-123-guid",
      reProcessingThresholdInMins = 60
    )
    
    // When
    val configString = config.toString
    
    // Then
    assert(configString.contains("adls://container/path"))
    assert(configString.contains("test-account-123"))
    assert(configString.contains("Incremental"))
    assert(configString.contains("job-123-guid"))
    assert(configString.contains("60"))
  }
} 