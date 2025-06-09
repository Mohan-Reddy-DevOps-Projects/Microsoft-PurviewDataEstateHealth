package com.microsoft.azurepurview.dataestatehealth.controls.util

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.mockito.ArgumentMatchers.anyString
import org.mockito.Mockito.{times, verify, when}
import org.scalatest.BeforeAndAfterAll
import org.scalatest.funsuite.AnyFunSuite
import org.scalatestplus.mockito.MockitoSugar

class ControlJobUtilsTest extends AnyFunSuite with BeforeAndAfterAll with MockitoSugar {
  
  // Create a SparkSession for testing
  private lazy val spark = SparkSession.builder()
    .appName("ControlJobUtilsTest")
    .master("local[*]")
    .config("spark.sql.shuffle.partitions", "1")
    .getOrCreate()
    
  private val logger = Logger.getLogger(getClass)
  private var mockLogger: Logger = _
  private var mockDf: DataFrame = _
  
  override def beforeAll(): Unit = {
    super.beforeAll()
    mockLogger = mock[Logger]
    mockDf = mock[DataFrame]
  }
  
  test("extractAccountName should extract correct account name from datasource FQN") {
    // Test case 1: Valid FQN with host - use exact format matching DATASOURCE_HOST_PATTERN
    val datasourceFQN1 = "https://testaccount.dfs.core.windows.net/"
    assert(ControlJobUtils.extractAccountName(datasourceFQN1) === "testaccount.dfs.core.windows.net")

    // Test case 2: Empty FQN
    assert(ControlJobUtils.extractAccountName("") === "")

    // Test case 3: Null FQN
    assert(ControlJobUtils.extractAccountName(null) === "")

    // Test case 4: Invalid FQN format
    assert(ControlJobUtils.extractAccountName("not-a-valid-fqn") === "")
    
    // Test case 5: Valid FQN with additional path segments
    val datasourceFQN2 = "https://testaccount.dfs.core.windows.net/container/path"
    assert(ControlJobUtils.extractAccountName(datasourceFQN2) === "testaccount.dfs.core.windows.net")
  }

  test("constructStorageEndpoint should create correct endpoint string") {
    // Test case 1: Valid inputs - use correct host format
    val fileSystem = "container1"
    val datasourceFQN = "https://testaccount.dfs.core.windows.net/"
    val folderPath = "folder/subfolder"

    val expectedEndpoint = "abfss://container1@testaccount.dfs.core.windows.net/folder/subfolder"
    assert(ControlJobUtils.constructStorageEndpoint(fileSystem, datasourceFQN, folderPath) === expectedEndpoint)

    // Test case 2: Null/empty inputs
    assert(ControlJobUtils.constructStorageEndpoint(null, datasourceFQN, folderPath) === "")
    assert(ControlJobUtils.constructStorageEndpoint(fileSystem, null, folderPath) === "")
    assert(ControlJobUtils.constructStorageEndpoint(fileSystem, datasourceFQN, null) === "abfss://container1@testaccount.dfs.core.windows.net/")
    
    // Test case 3: Empty file system
    assert(ControlJobUtils.constructStorageEndpoint("", datasourceFQN, folderPath) === "abfss://@testaccount.dfs.core.windows.net/folder/subfolder")
    
    // Test case 4: Invalid datasource FQN (no extraction possible)
    val invalidDatasourceFQN = "invalid-fqn"
    assert(ControlJobUtils.constructStorageEndpoint(fileSystem, invalidDatasourceFQN, folderPath) === "")
    
    // Test case 5: Path with leading and trailing slashes
    val folderPathWithSlashes = "/folder/subfolder/"
    assert(ControlJobUtils.constructStorageEndpoint(fileSystem, datasourceFQN, folderPathWithSlashes) === "abfss://container1@testaccount.dfs.core.windows.net//folder/subfolder/")
  }

  test("isDataFrameEmpty should correctly identify empty DataFrames") {
    import spark.implicits._
    
    // Create an empty DataFrame - use a more explicit approach
    val emptyDf = Seq.empty[(Int, String)].toDF("id", "value")
    assert(ControlJobUtils.isDataFrameEmpty(emptyDf))
    
    // Create a non-empty DataFrame - use more explicit approach
    val data = Seq((1, "test"))
    val nonEmptyDf = data.toDF("id", "value")
    assert(!ControlJobUtils.isDataFrameEmpty(nonEmptyDf))
    
    // Test with null DataFrame
    assert(ControlJobUtils.isDataFrameEmpty(null))
  }
  
  test("isDataFrameEmpty should handle exceptions correctly") {
    // Given a DataFrame that throws an exception when isEmpty is called
    val mockDf = mock[DataFrame]
    when(mockDf.isEmpty).thenThrow(new RuntimeException("DataFrame error"))
    
    // When/Then
    val exception = intercept[RuntimeException] {
      ControlJobUtils.isDataFrameEmpty(mockDf)
    }
    
    // Then
    assert(exception.getMessage.contains("Failed to check if DataFrame is empty"))
  }
  
  test("getDataFrameCount should return correct row count") {
    import spark.implicits._
    
    // Create a DataFrame with known row count
    val rowCount = 5
    val df = (1 to rowCount).map(i => (i, s"value$i")).toDF("id", "value")
    
    // When
    val result = ControlJobUtils.getDataFrameCount(df, logger)
    
    // Then
    assert(result === rowCount)
  }
  
  test("getDataFrameCount should throw RuntimeException on error") {
    // Given a DataFrame that throws an exception when count is called
    when(mockDf.count()).thenThrow(new RuntimeException("Count error"))
    
    // When/Then
    val exception = intercept[RuntimeException] {
      ControlJobUtils.getDataFrameCount(mockDf, mockLogger)
    }
    
    // Then
    assert(exception.getMessage.contains("Failed to get DataFrame count"))
  }
  
  test("getDataFrameSchema should return correct schema string") {
    import spark.implicits._
    
    // Create a DataFrame with known schema
    val df = Seq((1, "test")).toDF("id", "value")
    
    // When
    val schemaString = ControlJobUtils.getDataFrameSchema(df, logger)
    
    // Then
    assert(schemaString.contains("root"))
    assert(schemaString.contains("|-- id:"))
    assert(schemaString.contains("|-- value:"))
  }
  
  test("getDataFrameSchema should throw RuntimeException on error") {
    // Given a DataFrame that throws an exception when schema is accessed
    when(mockDf.schema).thenThrow(new RuntimeException("Schema error"))
    
    // When/Then
    val exception = intercept[RuntimeException] {
      ControlJobUtils.getDataFrameSchema(mockDf, mockLogger)
    }
    
    // Then
    assert(exception.getMessage.contains("Failed to get DataFrame schema"))
  }
  
  // Clean up SparkSession after tests
  override def afterAll(): Unit = {
    if (spark != null) {
      spark.stop()
    }
    super.afterAll()
  }
} 