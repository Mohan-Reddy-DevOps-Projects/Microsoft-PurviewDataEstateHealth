package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, DataFrameReader, RuntimeConfig, SparkSession}
import org.mockito.ArgumentMatchers.{any, anyString}
import org.mockito.Mockito.{times, verify, when}
import org.scalatest.BeforeAndAfterEach
import org.scalatest.funsuite.AnyFunSuite
import org.scalatestplus.mockito.MockitoSugar

class ColdStartSoftCheckTest extends AnyFunSuite with BeforeAndAfterEach with MockitoSugar {

  private var mockSpark: SparkSession = _
  private var mockLogger: Logger = _
  private var mockReader: DataFrameReader = _
  private var mockDataFrame: DataFrame = _
  private var mockRuntimeConfig: RuntimeConfig = _
  private var coldStartSoftCheck: ColdStartSoftCheck = _
  
  private val testContainerName = "test-container"
  private val testDatabase = "test-database"
  private val testEndpoint = "https://testcosmos.documents.azure.com:443/"
  private val testKey = "test-key-12345"

  override def beforeEach(): Unit = {
    super.beforeEach()
    mockSpark = mock[SparkSession]
    mockLogger = mock[Logger]
    mockReader = mock[DataFrameReader]
    mockDataFrame = mock[DataFrame]
    mockRuntimeConfig = mock[RuntimeConfig]
    
    // Configure the mockRuntimeConfig to return expected values when get is called
    when(mockRuntimeConfig.get(CosmosConfig.ACCOUNT_ENDPOINT_KEY)).thenReturn(testEndpoint)
    when(mockRuntimeConfig.get(CosmosConfig.ACCOUNT_KEY_KEY)).thenReturn(testKey)
    when(mockRuntimeConfig.get(CosmosConfig.DATABASE_KEY)).thenReturn(testDatabase)
    
    // Set up mockSpark to return mockRuntimeConfig when conf is called
    when(mockSpark.conf).thenReturn(mockRuntimeConfig)
    when(mockSpark.read).thenReturn(mockReader)
    when(mockReader.format(anyString())).thenReturn(mockReader)
    when(mockReader.option(anyString(), anyString())).thenReturn(mockReader)
    when(mockReader.load()).thenReturn(mockDataFrame)
    
    coldStartSoftCheck = new ColdStartSoftCheck(mockSpark, mockLogger)
  }

  test("validateCheckIn should return true when database connection is successful and data exists") {
    // Given
    when(mockDataFrame.isEmpty).thenReturn(false)
    
    // When
    val result = coldStartSoftCheck.validateCheckIn(testContainerName)
    
    // Then
    assert(result)
    verify(mockLogger).info(s"Performing cold start check for container: $testContainerName in database: $testDatabase")
    verify(mockLogger).info(s"Cold start check for $testContainerName result: Success")
    verify(mockReader).load()
    verify(mockDataFrame).isEmpty
  }
  
  test("validateCheckIn should return false when DataFrame is empty") {
    // Given
    when(mockDataFrame.isEmpty).thenReturn(true)
    
    // When
    val result = coldStartSoftCheck.validateCheckIn(testContainerName)
    
    // Then
    assert(!result)
    verify(mockLogger).info(s"Cold start check for $testContainerName result: Empty dataset")
  }
  
  test("validateCheckIn should return false when an exception occurs") {
    // Given
    when(mockReader.load()).thenThrow(new RuntimeException("Connection failed"))
    
    // When
    val result = coldStartSoftCheck.validateCheckIn(testContainerName)
    
    // Then
    assert(!result)
    verify(mockLogger).error(contains("Error reading from Cosmos DB container"), any[Exception])
  }
  
  test("validateCheckIn should throw IllegalArgumentException when container name is null") {
    // Given
    val nullContainerName: String = null
    
    // When/Then
    val exception = intercept[IllegalArgumentException] {
      coldStartSoftCheck.validateCheckIn(nullContainerName)
    }
    
    // Then
    assert(exception.getMessage.contains("Container name is null or empty"))
  }
  
  test("validateCheckIn should throw IllegalArgumentException when container name is empty") {
    // Given
    val emptyContainerName = ""
    
    // When/Then
    val exception = intercept[IllegalArgumentException] {
      coldStartSoftCheck.validateCheckIn(emptyContainerName)
    }
    
    // Then
    assert(exception.getMessage.contains("Container name is null or empty"))
  }
  
  test("validateCheckIn should use provided database name when specified") {
    // Given
    val customDatabase = "custom-database"
    when(mockDataFrame.isEmpty).thenReturn(false)
    
    // When
    val result = coldStartSoftCheck.validateCheckIn(testContainerName, customDatabase)
    
    // Then
    assert(result)
    verify(mockLogger).info(s"Performing cold start check for container: $testContainerName in database: $customDatabase")
  }
  
  private def contains(str: String) = {
    org.mockito.ArgumentMatchers.contains(str)
  }
} 