package com.microsoft.azurepurview.dataestatehealth.controls.constants

/**
 * Constants used throughout the Control Jobs module.
 * Organized into subobjects for related constants to improve maintainability.
 */
object Constants {
  /**
   * Cosmos DB configuration constants
   */
  object Cosmos {
    /** Container name for control job data */
    val CONTAINER = "DHControlJob"
    
    /** Database name for control system */
    val DATABASE = "dgh-Control"
  }
  
  /**
   * Rule evaluation status constants
   */
  object RuleStatus {
    /** Status value for passed rules */
    val PASS = "PASS"
    
    /** Status value for failed rules */
    val FAIL = "FAIL"
  }
  
  /**
   * SQL and DataFrame column name constants
   */
  object Columns {
    /** Rule identifier column */
    val RULE_ID = "id"
    
    /** Control identifier column */
    val CONTROL_ID = "controlId"
    
    /** SQL query column */
    val QUERY = "query"
    
    /** Rules definitions column */
    val RULES = "rules"
    
    /** Rule condition column */
    val CONDITION = "condition"
    
    /** Rule evaluation result column */
    val RESULT = "result"
    
    /** Map key column for rule results */
    val KEY = "key"
    
    /** Map value column for rule results */
    val VALUE = "value"
  }
  
  /**
   * Storage related constants
   */
  object Storage {
    /** Prefix for Azure Blob storage URLs */
    val ABFSS_PREFIX = "abfss://"
    
    /** Regex pattern to extract host from datasource FQN */
    val DATASOURCE_HOST_PATTERN = "https://([^/]+)/"
  }
  
  /**
   * Common file format constants
   */
  object FileFormat {
    /** Delta table format name */
    val DELTA = "delta"
    
    /** Parquet file format name */
    val PARQUET = "parquet"
  }
  
  /**
   * Business domain related constants
   */
  object BusinessDomain {
    /** Business domain ID placeholder */
    val DEH_DOMAIN_ID = "___deh_business_domain_id___"
  }
} 