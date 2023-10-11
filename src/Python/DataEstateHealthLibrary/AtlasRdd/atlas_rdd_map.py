class AtlasRddMap:

    curation_def_columns = [
        "DataAssetDescription",
        "Classifications",
        "DataOwner"
    ]
    
    PlatformProviderSourceMapping = {
  "Amazon Cloud": {
    "AWS": [
      "Amazon RDS Database (MySQL)",
      "Amazon RDS Database (PostgreSQL)",
      "Amazon RDS Database (SQL)",
      "aws"
    ]
  },
  "Google Cloud": {
    "GCP": [
      "Google BigQuery",
      "Looker"
    ]
  },
  "Microsoft Cloud": {
    "Azure": [
      "Azure Blob Storage",
      "Azure Cognitive Search",
      "Azure Cosmos DB",
      "Azure Data Explorer",
      "Azure Data Factory",
      "Azure Data Lake Storage Gen1",
      "Azure Data Lake Storage Gen2",
      "Azure Data Share",
      "Azure Database for MariaDB",
      "Azure Database for MySQL",
      "Azure Database for PostgreSQL",
      "Azure Files",
      "Azure Machine Learning Service",
      "Azure SQL Data Warehouse",
      "Azure SQL Database",
      "Azure SQL Managed Instance",
      "Azure Synapse Analytics",
      "Azure Table Storage"
    ],
    "Power BI": [
      "Power BI"
    ]
  },
  "Others": {
    "IBM": [
      "DB2"
    ],
    "Oracle": [
      "Oracle"
    ],
    "Salesforce": [
      "Salesforce"
    ],
    "SAP": [
      "SAP BW",
      "SAP ECC",
      "SAP HANA",
      "SAP S4HANA"
    ],
    "Snowflake": [
      "Snowflake"
    ],
    "Teradata": [
      "Teradata"
    ]
  }
}
    
    ObjectTypeMapping = {
        "adf_copy_activity" : "Data pipelines",
        "adf_dataflow_activity" : "Data pipelines",
        "adf_executeSsisPackage_activity" : "Data pipelines",
        "adf_pipeline" : "Data pipelines",
        "ads_org_dataset" : "Tables",
        "ads_share" : "Data pipelines",
        "aws_rds_mysql_table" : "Tables",
        "aws_rds_mysql_view" : "Tables",
        "aws_rds_postgresql_table" : "Tables",
        "aws_rds_postgresql_view" : "Tables",
        "aws_rds_sql_table" : "Tables",
        "aws_rds_sql_view" : "Tables",
        "aws_s3_v2_bucket" : "Folders",
        "aws_s3_v2_directory" : "Folders",
        "aws_s3_v2_object" : "Files",
        "aws_s3_v2_resource_set" : "Files",
        "azure_blob_container" : "Folders",
        "azure_blob_path" : "FolderOrFile",
        "azure_blob_resource_set" : "Files",
        "azure_datalake_gen1_path" : "FolderOrFile",
        "azure_datalake_gen1_resource_set" : "Files",
        "azure_datalake_gen2_filesystem" : "Folders",
        "azure_datalake_gen2_path" : "FolderOrFile",
        "azure_datalake_gen2_resource_set" : "Files",
        "azure_data_explorer_table" : "Tables",
        "azure_file" : "File",
        "azure_file_directory" : "Folders",
        "azure_file_resource_set" : "Files",
        "azure_file_share" : "Folders",
        "azure_mariadb_table" : "Tables",
        "azure_mariadb_view" : "Tables",
        "azure_mysql_table" : "Tables",
        "azure_mysql_view" : "Tables",
        "azure_postgresql_table" : "Tables",
        "azure_postgresql_view" : "Tables",
        "azure_sql_dw_table" : "Tables",
        "azure_sql_dw_view" : "Tables",
        "azure_sql_mi_table" : "Tables",
        "azure_sql_mi_view" : "Tables",
        "azure_sql_table" : "Tables",
        "azure_sql_view" : "Tables",
        "azure_synapse_dedicated_sql_table" : "Tables",
        "azure_synapse_dedicated_sql_view" : "Tables",
        "azure_synapse_pipeline" : "Data pipelines",
        "azure_synapse_serverless_sql_table" : "Tables",
        "azure_synapse_serverless_sql_view" : "Tables",
        "azure_synapse_copy_activity" : "Data pipelines",
        "azure_synapse_dataflow_activity" : "Data pipelines",
        "azure_table" : "Tables",
        "bigquery_table" : "Tables",
        "bigquery_view" : "Tables",
        "cassandra_materialized_view" : "Tables",
        "cassandra_table" : "Tables",
        "erwin_entity" : "Tables",
        "erwin_stored_procedure" : "Stored procedures",
        "erwin_view" : "Tables",
        "hive_table": "Tables",
        "hive_view" : "Tables",
        "looker_dashboard" : "Dashboards",
        "looker_look" : "Reports",
        "looker_view" : "Tables",
        "mongodb_collection": " Tables",
        "mongodb_view":"Tables",
        "mssql_table" : "Tables",
        "mssql_view" : "Tables",
        "oracle_function" : "Stored procedures",
        "oracle_stored_procedure" : "Stored procedures",
        "oracle_table" : "Tables",
        "oracle_view" : "Tables",
        "powerbi_dashboard" : "Dashboards",
        "powerbi_dataflow" : "Data pipelines",
        "powerbi_dataset" : "Tables",
        "powerbi_report" : "Reports",
        "sap_s4hana_table" : "Tables",
        "sap_s4hana_view" : "Tables",
        "sap_ecc_table" : "Tables",
        "sap_ecc_view" : "Tables",
        "ssis_package" : "Data pipelines",
        "teradata_function" : "Stored procedures",
        "teradata_view" : "Tables",
        "teradata_stored_procedure" : "Stored procedures",
        "teradata_table" : "Tables"
    }
    
    reason_for_unclassified_codes = {
        "low_confidence": "CONFIDENCE_BELOW_THRESHOLD",
        "no_match": "NO_MATCH_FOUND",
        "no_selection": "ASSET_NOT_SELECTED",
        "processing_error": "PROCESSING_ERROR",
        "non_leaf": "NON_APPLICABLE"
    }

    TypeNameFileList = [
        "azure_blob_path",
        "azure_datalake_gen1_path",
        "azure_datalake_gen2_path",
        "ms_cosmos_path",
        "azure_file",
        "aws_s3_v2_object",
        "azure_blob_resource_set",
        "azure_datalake_gen1_resource_set",
        "azure_datalake_gen2_resource_set",
        "azure_file_resource_set",
        "ms_cosmos_resource_set"
]
