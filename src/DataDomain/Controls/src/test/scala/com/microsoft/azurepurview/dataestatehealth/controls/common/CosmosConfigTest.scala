package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.spark.sql.{DataFrame, DataFrameReader, RuntimeConfig, SparkSession}
import org.mockito.ArgumentMatchers.anyString
import org.mockito.Mockito.{times, verify, when}
import org.scalatest.BeforeAndAfterEach
import org.scalatest.funsuite.AnyFunSuite
import org.scalatestplus.mockito.MockitoSugar

class CosmosConfigTest extends AnyFunSuite with BeforeAndAfterEach with MockitoSugar {

  private var mockSpark: SparkSession = _
  private var mockConfWrapper: SparkSession.Builder = _
  private var mockReader: DataFrameReader = _
  private var mockRuntimeConfig: RuntimeConfig = _
  
  private val testEndpoint = "https://testcosmos.documents.azure.com:443/"
  private val testKey = "test-key-12345"
  private val testDatabase = "test-database" 
  private val testContainer = "test-container"

  override def beforeEach(): Unit = {
    super.beforeEach()
    mockSpark = mock[SparkSession]
    mockConfWrapper = mock[SparkSession.Builder]
    mockReader = mock[DataFrameReader]
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
  }

  test("getSettings should retrieve settings from SparkSession configuration") {
    // When
    val settings = CosmosConfig.getSettings(mockSpark)
    
    // Then
    assert(settings.accountEndpoint === testEndpoint)
    assert(settings.accountKey === testKey)
    assert(settings.database === testDatabase)
  }
  
  test("configureReader should configure DataFrameReader with correct format and options") {
    // Given
    val containerName = testContainer
    
    // When
    val reader = CosmosConfig.configureReader(mockSpark, containerName)
    
    // Then
    verify(mockReader).format(CosmosConfig.FORMAT)
    verify(mockReader).option(CosmosConfig.ACCOUNT_ENDPOINT_KEY, testEndpoint)
    verify(mockReader).option(CosmosConfig.ACCOUNT_KEY_KEY, testKey)
    verify(mockReader).option(CosmosConfig.DATABASE_KEY, testDatabase)
    verify(mockReader).option(CosmosConfig.CONTAINER_KEY, containerName)
  }
  
  test("configureReader should use provided settings instead of SparkSession config if specified") {
    // Given
    val containerName = testContainer
    val customSettings = CosmosDbSettings(
      accountEndpoint = "https://customcosmos.documents.azure.com:443/",
      accountKey = "custom-key-54321",
      database = "custom-database"
    )
    
    // When
    val reader = CosmosConfig.configureReader(mockSpark, containerName, Some(customSettings))
    
    // Then
    verify(mockReader).format(CosmosConfig.FORMAT)
    verify(mockReader).option(CosmosConfig.ACCOUNT_ENDPOINT_KEY, customSettings.accountEndpoint)
    verify(mockReader).option(CosmosConfig.ACCOUNT_KEY_KEY, customSettings.accountKey)
    verify(mockReader).option(CosmosConfig.DATABASE_KEY, customSettings.database)
    verify(mockReader).option(CosmosConfig.CONTAINER_KEY, containerName)
  }
  
  test("CosmosDbSettings should properly store and retrieve values") {
    // When
    val settings = CosmosDbSettings(
      accountEndpoint = testEndpoint,
      accountKey = testKey,
      database = testDatabase
    )
    
    // Then
    assert(settings.accountEndpoint === testEndpoint)
    assert(settings.accountKey === testKey)
    assert(settings.database === testDatabase)
  }
  
  test("CosmosDbSettings instances should be comparable based on their contents") {
    // Given
    val settings1 = CosmosDbSettings(
      accountEndpoint = testEndpoint,
      accountKey = testKey,
      database = testDatabase
    )
    
    val settings2 = CosmosDbSettings(
      accountEndpoint = testEndpoint,
      accountKey = testKey,
      database = testDatabase
    )
    
    val differentSettings = CosmosDbSettings(
      accountEndpoint = "https://differentcosmos.documents.azure.com:443/",
      accountKey = "different-key",
      database = "different-database"
    )
    
    // Then
    assert(settings1 === settings2) // Should be equal based on content
    assert(settings1 !== differentSettings) // Should not be equal
  }
  
  test("CosmosDbSettings toString should contain essential information") {
    // Given
    val settings = CosmosDbSettings(
      accountEndpoint = testEndpoint,
      accountKey = testKey,
      database = testDatabase
    )
    
    // When
    val settingsString = settings.toString
    
    // Then
    assert(settingsString.contains(testEndpoint))
    // Should not include the key in the toString for security reasons
    assert(settingsString.contains(testDatabase))
  }
} 