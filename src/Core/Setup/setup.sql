USE [@databaseName];

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'ParquetFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT ParquetFormat WITH (  FORMAT_TYPE = PARQUET );
END

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'DeltaLakeFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT DeltaLakeFormat WITH (  FORMAT_TYPE = DELTA );
END


DECLARE @DynamicSQL NVARCHAR(MAX);
DECLARE @DomainSchema NVARCHAR(512) = '@schemaName.DomainModel';
DECLARE @DimensionalSchema NVARCHAR(512) = '@schemaName.DimensionalModel';
DECLARE @ContainerSchema NVARCHAR(512) = '@schemaName';
DECLARE @sql NVARCHAR(MAX) = '';


-- Generate drop statements for all tables
SELECT @sql += 'DROP EXTERNAL TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
and ( TABLE_SCHEMA = @DomainSchema or TABLE_SCHEMA = @DimensionalSchema or TABLE_SCHEMA = @ContainerSchema)
--drop from all schemas

print @sql

-- Execute the generated SQL
EXEC SP_EXECUTESQL @sql;

-- Drop all views
SET @sql = '';

-- Generate drop statements for all views
SELECT @sql += 'DROP VIEW [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.VIEWS
WHERE ( TABLE_SCHEMA = @DomainSchema or TABLE_SCHEMA = @DimensionalSchema or TABLE_SCHEMA = @ContainerSchema)
print @sql

-- Execute the generated SQL
EXEC SP_EXECUTESQL @sql;

IF EXISTS(SELECT [name] FROM sys.external_data_sources WHERE [name] = '@containerName')
BEGIN
   PRINT 'External Data Source Dropped'
   DROP EXTERNAL DATA SOURCE [@containerName];   
END

IF not EXISTS(SELECT [name] FROM sys.external_data_sources WHERE [name] = '@containerName')
BEGIN
    CREATE EXTERNAL DATA SOURCE [@containerName]
    WITH (LOCATION = @containerUri, CREDENTIAL = [@databaseScopedCredential]);
END


--BEGIN TRAN

/* SCHEMA Creation a part of provisioning hence commented.

IF NOT EXISTS (SELECT * FROM SYS.schemas WHERE name = @DomainSchema)
BEGIN
    SET @DynamicSQL = 'CREATE SCHEMA [' + @DomainSchema + ']';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'Schema created: ' + @DomainSchema;
END

IF NOT EXISTS (SELECT * FROM SYS.schemas WHERE name = @DimensionalSchema)
BEGIN
    SET @DynamicSQL = 'CREATE SCHEMA [' + @DimensionalSchema + ']';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'Schema created: ' + @DimensionalSchema;
END

*/

/*
DOMAIN MODEL SECTION
*/

PRINT 'STARTING CREATION OF DOMAIN TABLES...'

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'AccessPolicyResourceType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicyResourceType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: AccessPolicyResourceType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicyResourceType]
(
     [ResourceTypeId] [nvarchar](512),
     [ResourceTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/AccessPolicyResourceType/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: AccessPolicyResourceType';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'AccessPolicyProvisioningState'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicyProvisioningState]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: AccessPolicyProvisioningState';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicyProvisioningState]
(
     [ProvisioningStateId] [nvarchar](256),
     [ProvisioningStateDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/AccessPolicyProvisioningState/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: AccessPolicyProvisioningState';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'Relationship'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[Relationship]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: Relationship';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[Relationship]
(
    [AccountId] [nvarchar](512),
	[Type] [nvarchar](512),
	[SourceType] [nvarchar](512),
	[SourceId] [nvarchar](512),
	[TargetType] [nvarchar](512),
	[TargetId] [nvarchar](512),
	[ModifiedByUserId] [nvarchar](512),
	[ModifiedDateTime] [datetime2](7),
	[EventProcessingTime] [bigint],
	[OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/Relationship/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: Relationship';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'GlossaryTermBusinessDomainAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTermBusinessDomainAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: GlossaryTermBusinessDomainAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTermBusinessDomainAssignment]
(
    [GlossaryTermID] [nvarchar](512),
       [BusinessDomainId] [nvarchar](512),
       [AssignedByUserId]  [nvarchar](512),
       [AssignmentDatetime]  [datetime2],
       [GlossaryTermStatus] [nvarchar](512),
       [ActiveFlag] [int],
       [ActiveFlagLastModifiedDatetime]  [datetime2],
       [CreatedDatetime]  [datetime2],
       [CreatedByUserId]  [nvarchar](512),
       [ModifiedDateTime]  [datetime2],
       [ModifiedByUserId]  [nvarchar](512),
       [EventProcessingTime] [bigint],
	   [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/GlossaryTermBusinessDomainAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: GlossaryTermBusinessDomainAssignment';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'GlossaryTerm'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTerm]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: GlossaryTerm';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTerm]
(
    [GlossaryTermId] [nvarchar](512),
     [ParentGlossaryTermId] [nvarchar](512),
     [GlossaryTermDisplayName] [nvarchar](512),
     [GlossaryDescription] [nvarchar](max),
     [AccountId] [nvarchar](512),
     [Status] [nvarchar](50),
     [IsLeaf] [Int],
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](512),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](512),
     [EventProcessingTime] [bigint],
	 [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/GlossaryTerm/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: GlossaryTerm';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityRuleType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityRuleType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleType]
(
     [RuleTypeId] [nvarchar](256),
     [RuleTypeDisplayName] [nvarchar](512),
     [RuleTypeDesc] [nvarchar](512),
     [DimensionDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityRuleType/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityRuleType';



IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityRuleColumnExecution'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleColumnExecution]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityRuleColumnExecution';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleColumnExecution]
(
    [JobExecutionId] [nvarchar](256),
     [RuleId] [nvarchar](256),
     [DataAssetId] [nvarchar](512),
     [ColumnId] [nvarchar](256),
     [ColumnResultScore] [decimal](18,10),
     [RowPassCount] [Int],
     [RowFailCount] [Int],
     [RowMiscastCount] [Int],
     [RowEmptyCount] [Int],
     [RowIgnoredCount] [Int]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityRuleColumnExecution/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityRuleColumnExecution';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityRule'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRule]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityRule';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRule]
(
      [RuleId] [nvarchar](256),
      [SourceRuleId] [nvarchar](256),
      [BusinessDomainId] [nvarchar](512),
      [DataProductId] [nvarchar](512),
      [DataAssetId] [nvarchar](512),
      [RuleTypeId] [nvarchar](256),
      [RuleOriginDisplayName] [nvarchar](512),
      [RuleTargetObjectType] [nvarchar](512),
      [RuleDisplayName] [nvarchar](512),
      [RuleDescription] [nvarchar](max),
      [Status] [nvarchar](256),	  
      [AccountId] [nvarchar](256),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [nvarchar](256),
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](256),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityRule/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityRule';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityJobExecution'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityJobExecution]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityJobExecution';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityJobExecution]
(
	 [JobExecutionId] [nvarchar](256),
     [JobExecutionStatusDisplayName] [nvarchar](512),
     [JobType] [nvarchar](512),
     [ScanTypeDisplayName] [nvarchar](512),
     [JobCreationDatetime] [datetime2],
     [ExecutionStartDatetime] [datetime2],
     [ExecutionEndDatetime] [datetime2]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityJobExecution/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityJobExecution';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityAssetRuleExecution'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityAssetRuleExecution]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityAssetRuleExecution';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityAssetRuleExecution]
(
    [JobExecutionId] [nvarchar](256),
     [RuleId] [nvarchar](256),
     [DataAssetId] [nvarchar](512),
     [AssetResultScore] [decimal](18,10),
     [RowPassCount] [Int],
     [RowFailCount] [Int],
     [RowMiscastCount] [Int],
     [RowEmptyCount] [Int],
     [RowIgnoredCount] [Int]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityAssetRuleExecution/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityAssetRuleExecution';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductUpdateFrequency'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductUpdateFrequency]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductUpdateFrequency';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductUpdateFrequency]
(
    [UpdateFrequencyId] [nvarchar](512),
    [UpdateFrequencyDisplayName] [nvarchar](512),
    [SortOrder] [int]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductUpdateFrequency/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductUpdateFrequency';

IF EXISTS (
        SELECT * 
        FROM sys.tables 
        WHERE [name] = 'DataProductType'
        AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
    )
    BEGIN 
        SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductType]';
        EXEC sp_executesql @DynamicSQL;
        PRINT 'External table dropped: DataProductType';
    END

    SET @DynamicSQL = '
        CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductType]
        (
            [DataProductTypeID] [nvarchar](256),
            [DataProductTypeDisplayName] [nvarchar](512),
            [DataProductTypeDescription] [nvarchar](max)
        )
        WITH (
            DATA_SOURCE = [@containerName],
            LOCATION = N''DomainModel/DataProductType/'',
            FILE_FORMAT = [DeltaLakeFormat]
        )';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table created: DataProductType';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductTermsOfUse'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductTermsOfUse]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductTermsOfUse';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductTermsOfUse]
(
     [DataProductId] [nvarchar](512),
     [TermsOfUseId] [nvarchar](256),
     [TermsOfUseDisplayName] [nvarchar](512),
     [TermsOfUseHyperlink] [nvarchar](512),
     [DataAssetId] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductTermsOfUse/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductTermsOfUse';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductStatus'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductStatus]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductStatus';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductStatus]
(
    [DataProductStatusID] [nvarchar](256),
    [DataProductStatusDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductStatus/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductStatus';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductDocumentation'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductDocumentation]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductDocumentation';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductDocumentation]
(
    [DataProductId] [nvarchar](512),
     [DocumentationId] [nvarchar](256),
     [DocumentationDisplayName] [nvarchar](512),
     [DocumentationHyperlink] [nvarchar](512),
     [DataAssetId] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductDocumentation/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductDocumentation';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductBusinessDomainAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductBusinessDomainAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductBusinessDomainAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductBusinessDomainAssignment]
(
    [DataProductID] [nvarchar](512),
     [BusinessDomainId] [nvarchar](512),
     [AssignedByUserId] [nvarchar](512),
     [AssignmentDateTime] [datetime2],
     [ActiveFlag] [int],
     [ActiveFlagLastModifiedDateTime] [datetime2]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductBusinessDomainAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductBusinessDomainAssignment';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductAssetAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductAssetAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductAssetAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductAssetAssignment]
(
      [DataProductId] [nvarchar](512),
	  [DataAssetId] [nvarchar](512),
      [AssignedByUserId] [nvarchar](512),
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](512),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductAssetAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductAssetAssignment';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProductOwner'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProductOwner]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProductOwner';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProductOwner]
(
    [DataProductId] [nvarchar](512),
    [DataProductOwnerId] [nvarchar](512),
    [DataProductOwnerDescription] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProductOwner/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProductOwner';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataProduct'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataProduct]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataProduct';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataProduct]
(
    [DataProductID] [nvarchar](512),
      [DataProductDisplayName] [nvarchar](512),
      [DataProductDescription] [nvarchar](max),
      [AccountId] [nvarchar](512),
      [DataProductTypeID] [nvarchar](256),
      [UseCases] [nvarchar](max),
      [SensitivityLabel] [nvarchar](512),
	  [Endorsed] [bit],
      [TermsOfUseHyperlink] [nvarchar](512),
      [DocumentationHyperlink] [nvarchar](512),
      [ExpiredFlag] [int],
      [ExpiredFlagLastModifiedDatetime] [datetime2],
      [DataProductStatusID] [nvarchar](256),
      [DataProductStatusLastUpdatedDatetime] [datetime2],
      [UpdateFrequencyId] [nvarchar](512),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [nvarchar](512),
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](512),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataProduct/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataProduct';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetTypeDataType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetTypeDataType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetTypeDataType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetTypeDataType]
(
     [DataTypeId] [nvarchar] (256),
	 [DataAssetTypeId] [nvarchar] (256),
     [DataTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetTypeDataType/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetTypeDataType';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetType]
(
    [DataAssetTypeId] [nvarchar](256),
     [DataAssetTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetType/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetType';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetOwnerAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetOwnerAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetOwnerAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetOwnerAssignment]
(
    [DataAssetId] [nvarchar](512),
	  [DataAssetOwnerId] [nvarchar](256),
      [AssignedByUserId] [nvarchar](512),	  
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](512),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetOwnerAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetOwnerAssignment';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetOwner'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetOwner]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetOwner';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetOwner]
(
    [DataAssetOwnerId] [nvarchar](256),
     [DataAssetOwner] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetOwner/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetOwner';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetDomainAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetDomainAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetDomainAssignment';
END

-- Creating the external table DataAssetDomainAssignment
SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetDomainAssignment]
(
    [DataAssetId] [nvarchar](512),
	  [BusinessDomainId] [nvarchar](512),
      [AssignedByUserId] [nvarchar](512),	  
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](512),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetDomainAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetDomainAssignment';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetColumnClassificationAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetColumnClassificationAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetColumnClassificationAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetColumnClassificationAssignment]
(
     [DataAssetId] [nvarchar] (256),
	 [ColumnId] [nvarchar] (256),
	 [ClassificationId] [nvarchar] (256),
     [ModifiedDateTime] [datetime2]
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetColumnClassificationAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetColumnClassificationAssignment';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAssetColumn'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetColumn]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAssetColumn';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAssetColumn]
(
    [DataAssetId] [nvarchar](512),
      [ColumnId] [nvarchar](256),
      [ColumnDisplayName] [nvarchar](512),
      [ColumnDescription] [nvarchar](512),
      [DataAssetTypeId] [nvarchar](256),
      [DataTypeId] [nvarchar](256),
      [AccountId] [nvarchar](256),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [nvarchar](512),
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](512),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50),
      [ClassificationId] [nvarchar](512),
      [ColumnDataType] [nvarchar](50),
      [ColumnClassification] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAssetColumn/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAssetColumn';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataAsset'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataAsset]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataAsset';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataAsset]
(
    [DataAssetId] [nvarchar](512),
      [DataAssetTypeId] [nvarchar](256),
      [AssetDisplayName] [nvarchar](512),
      [AssetDescription] [nvarchar](max),
      [FullyQualifiedName] [nvarchar](512),	  
      [ScanSource] [nvarchar](512),
      [IsCertified] [int],
      [DataAssetStatusLastUpdatedDatetime] [datetime2],
      [DataAssetLastUpdatedByUserId] [nvarchar](256),
      [AccountId] [nvarchar](256),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [nvarchar](256),
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](256),
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataAsset/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataAsset';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataQualityRuleType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataQualityRuleType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataQualityRuleType]
(
    [AccessPolicySetId] [nvarchar](512),
     [AccessUseCaseDisplayName] [nvarchar](512),
     [AccessUseCaseDescription] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataQualityRuleType/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataQualityRuleType';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'Classification'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[Classification]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: Classification';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[Classification]
(
    [ClassificationId] [nvarchar] (256),
	 [ClassificationDisplayName] [nvarchar] (256),
	 [ClassificationDescription] [nvarchar] (256)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/Classification/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: Classification';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'BusinessDomain'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[BusinessDomain]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: BusinessDomain';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[BusinessDomain]
(
    [BusinessDomainId] [nvarchar](256),
     [ParentBusinessDomainId] [nvarchar](512),
     [BusinessDomainName] [nvarchar](512),
     [BusinessDomainDisplayName] [nvarchar](512),
     [BusinessDomainDescription] [nvarchar](max),
     [Status] [nvarchar](50),
     [IsRootDomain] [bit],
     [HasValidOwner] [bit],
     [AccountId] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](512),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](512),
     [EventProcessingTime] [bigint],
	 [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/BusinessDomain/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: BusinessDomain';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'DataSubscriberRequest'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[DataSubscriberRequest]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DataSubscriberRequest';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[DataSubscriberRequest]
(
    [SubscriberRequestId] [nvarchar](512),
     [DataProductId] [nvarchar](512),
     [BusinessDomainId] [nvarchar](512),
     [AccessPolicySetId] [nvarchar](512),
     [SubscriberIdentityTypeDisplayName] [nvarchar](512),
     [RequestorIdentityTypeDisplayName] [nvarchar](512),
     [SubscriberRequestStatus] [nvarchar](512),
     [RequestStatusDisplayName] [nvarchar](512),
     [SubscribedByUserId] [nvarchar](512),
     [SubscribedByUserTenantId] [nvarchar](512),
     [SubscribedByUserEmail] [nvarchar](512),
     [RequestedByUserId] [nvarchar](512),
     [RequestedByUserTenantId] [nvarchar](512),
     [RequestedByUserEmail] [nvarchar](512),
     [RequestWriteAccess] [int] ,
     [RequestAccessDecisionDateTime] [datetime2],
     [Version] [nvarchar](512),
     [AccountId] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](256),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](256),
     [EventProcessingTime] [bigint],
     [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/DataSubscriberRequest/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DataSubscriberRequest';

-- Dropping the external table PolicySetApprover if it exists
IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'PolicySetApprover'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[PolicySetApprover]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: PolicySetApprover';
END

-- Creating the external table PolicySetApprover
SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[PolicySetApprover]
(
    [SubscriberRequestId] [nvarchar](512),
     [AccessPolicySetId] [nvarchar](512),
     [ApproverUserId] [nvarchar](512),
     [ApproverIdentityType] [nvarchar](512),
     [ApproverUserTenantId] [nvarchar](512),
     [ApproverUserEmail] [nvarchar](512),
     [TermsOfUseRequired] [int] ,
     [PartnerExposurePermitted] [int] ,
     [CustomerExposurePermitted] [int] ,
     [PrivacyComplianceApprovalRequired] [int] ,
     [ManagerApprovalRequired] [int],
     [DataCopyPermitted] [int],
     [AccountId] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](256),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](256),
     [EventProcessingTime] [bigint],
     [OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/PolicySetApprover/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: PolicySetApprover';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'AccessPolicySet'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicySet]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: AccessPolicySet';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[AccessPolicySet]
(
    [AccessPolicySetId] [nvarchar](512),
      [ResourceTypeId] [nvarchar](256),
      [ProvisioningStateId] [nvarchar](256),
      [ActiveFlag] [int],
      [UseCaseExternalSharingPermittedFlag] [int],
      [UseCaseInternalSharingPermittedFlag] [int],
      [AttestationAcknowledgeTermsOfUseRequiredFlag] [int],
      [AttestationDataCopyPermittedFlag] [int],
      [PrivacyApprovalRequiredFlag] [int],
      [ManagerApprovalRequiredFlag] [int],
      [WriteAccessPermittedFlag] [int],
      [PolicyAppliedOnId] [nvarchar](512),
      [PolicyAppliedOn] [nvarchar](256),
      [AccessPolicySetVersion] [int],
      [AccountId] [nvarchar](512),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [nvarchar](256),
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [nvarchar](256),
      [EventProcessingTime] [bigint],
      [OperationType] [nvarchar](50),
      [ProvisioningStateDisplayName] [nvarchar](512),
      [AccessUseCaseDisplayName] [nvarchar](512),
      [AccessUseCaseDescription] [nvarchar](512),
      [ResourceType] [nvarchar](512)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/AccessPolicySet/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: AccessPolicySet';

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'GlossaryTermDataProductAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTermDataProductAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: GlossaryTermDataProductAssignment';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[GlossaryTermDataProductAssignment]
(
    [GlossaryTermID] [nvarchar](512),
	[DataProductId] [nvarchar](512),
	[ActiveFlag] [int],
	[ModifiedDateTime] [datetime2](7),
	[ModifiedByUserId] [nvarchar](512),
	[EventProcessingTime] [bigint],
	[OperationType] [nvarchar](50)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/GlossaryTermDataProductAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: GlossaryTermDataProductAssignment';


IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'CustomAccessUseCase'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[CustomAccessUseCase]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: CustomAccessUseCase';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[CustomAccessUseCase]
(
    [AccessPolicySetId] [nvarchar](512),
	[AccessUseCaseDisplayName] [nvarchar](512),
	[AccessUseCaseDescription] [nvarchar](512)
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/CustomAccessUseCase/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: CustomAccessUseCase';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[CDE]
(
    Name [nvarchar](512),
    DataType [nvarchar](50),
    Status [nvarchar](50),
    Description [nvarchar](max),
    CDEId [nvarchar](50),
    AccountId [nvarchar](50),
    CreatedDatetime [datetime2],
    CreatedByUserId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10),
    BusinessDomainId [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/CDE/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: CDE';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[CDEColumnAssignment]
(
    CDEId [nvarchar](50),
    ColumnId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/CDEColumnAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: CDEColumnAssignment';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[CDEDataProductAssignment]
(
    CDEId [nvarchar](50),
    DataProductId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/CDEDataProductAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: CDEDataProductAssignment';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[CDEGlossaryTermAssignment]
(
    CDEId [nvarchar](50),
    GlossaryTermId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/CDEGlossaryTermAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: CDEGlossaryTermAssignment';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[Action]
(
    ActionId [nvarchar](50),
    AccountId [nvarchar](50),
    BusinessDomainId [nvarchar](50),
    Category [nvarchar](200),
    Severity [nvarchar](10),
    FindingId [nvarchar](150),
    FindingName [nvarchar](500),
    Reason [nvarchar](4000),
    Recommendation [nvarchar](4000),
    FindingType [nvarchar](100),
    FindingSubType [nvarchar](100),
    TargetEntityType [nvarchar](100),
    Type [nvarchar](100),
    Status [nvarchar](50),
    CreatedDatetime [datetime2],
    CreatedByUserId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/Action/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: Action';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[OKR]
(
    OKRId [nvarchar](50),
    OKRDefintion [nvarchar](4000),
    Status [nvarchar](50),
    TargetDate [datetime2],
    AccountId [nvarchar](50),
    CreatedDatetime [datetime2],
    CreatedByUserId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10),
    BusinessDomainId [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/OKR/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: OKR';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[KeyResult]
(
    KeyResultId [nvarchar](50),
    KeyResultDefintion [nvarchar](4000),
    Status [nvarchar](50),
    Progress [int],
    Goal [int],
    Max [int],
    AccountId [nvarchar](50),
    CreatedDatetime [datetime2],
    CreatedByUserId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10),
    BusinessDomainId [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/KeyResult/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: KeyResult';

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DomainSchema + '].[OKRKeyResultAssignment]
(
    OKRId [nvarchar](50),
    KeyResultId [nvarchar](50),
    ModifiedDateTime [datetime2],
    ModifiedByUserId [nvarchar](50),
    EventProcessingTime [bigint],
    OperationType [nvarchar](10)
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DomainModel/OKRKeyResultAssignment/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: OKRKeyResultAssignment';

PRINT 'DOMAIN TABLES CREATION COMPLETE!'
/*
DIMENSIONAL MODEL SECTION
*/
PRINT 'STARTING CREATION OF DIMENSIONAL TABLES...'

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimBusinessDomain'
		AND schema_id IN (select schema_id from sys.schemas where name = @DimensionalSchema))

BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimBusinessDomain]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimBusinessDomain';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimBusinessDomain]
(
    [BusinessDomainId] [nvarchar](512),
     [BusinessDomainSourceId] [nvarchar](256),
     [BusinessDomainDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
     
)
WITH (DATA_SOURCE = [@containerName], LOCATION = N''DimensionalModel/DimBusinessDomain/'', FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimBusinessDomain';


IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDataProduct'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataProduct]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDataProduct';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataProduct]
(
    [DataProductId] [nvarchar](512),
	[DataProductSourceId] [nvarchar](256),
	[DataProductDisplayName] [nvarchar](512),
	[DataProductStatus] [nvarchar](50),
	[CreatedDatetime] [datetime2](7),
	[ModifiedDatetime] [datetime2](7),
	[IsActive] [bit]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N''DimensionalModel/DimDataProduct/'',FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDataProduct';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDataAsset'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataAsset]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDataAsset';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataAsset]
(
     [DataAssetId] [nvarchar](512),
     [DataAssetSourceId] [nvarchar](256),
     [DataAssetDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N''DimensionalModel/DimDataAsset/'',FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDataAsset';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDataAssetColumn'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataAssetColumn]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDataAssetColumn';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataAssetColumn]
(
    [DataAssetColumnId] [nvarchar](512),
     [DataAssetColumnSourceId] [nvarchar](256),
     [DataAssetColumnDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N''DimensionalModel/DimDataAssetColumn/'',FILE_FORMAT = [DeltaLakeFormat])';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDataAssetColumn';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDQScanProfile'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQScanProfile]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDQScanProfile';
END
SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQScanProfile]
(
     [DQScanProfileId] [Int],
     [RuleOriginDisplayName] [nvarchar](512),
     [RuleAppliedOn] [nvarchar](512)
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDQScanProfile/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDQScanProfile';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDQRuleType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQRuleType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDQRuleType';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQRuleType]
(
     [DQRuleTypeId] [int],
     [DQRuleTypeSourceId] [nvarchar](256),
     [DQRuleTypeDisplayName] [nvarchar](512),
     [QualityDimension] [nvarchar](512),
     [QualityDimensionCustomIndicator] [nvarchar](256)
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDQRuleType/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDQRuleType';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDQRuleName'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQRuleName]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDQRuleName';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQRuleName]
(
     [DQRuleId] [Int],
     [DQRuleNameId] [nvarchar](256),
     [DQRuleNameSourceId] [nvarchar](256),
     [DQRuleNameDisplayName] [nvarchar](512)
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDQRuleName/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDQRuleName';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDataHealthControl'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataHealthControl]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDataHealthControl';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDataHealthControl]
(
     [HealthControlId] [int],
     [HealthControlDisplayName] [nvarchar](256),
     [HealthControlGroupDisplayName] [nvarchar](512),
     [HealthControlDefinition] [nvarchar](256),
     [HealthControlScope] [nvarchar](256),
     [CDMCControlMapping] [nvarchar](256),
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDataHealthControl/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDataHealthControl';

-- Drop external table DimDQJobType if it exists
IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDQJobType'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQJobType]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDQJobType';
END

-- Create external table DimDQJobType
SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDQJobType]
(
    [JobTypeId] [int],
	[JobTypeDisplayName] [nvarchar](256)
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDQJobType/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDQJobType';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'DimDate'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDate]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: DimDate';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[DimDate]
(
      [DateId] [int],
      [YearId] [int],
      [YearName] [nvarchar](256),
      [CalendarSemesterId] [int],
      [CalendarSemesterDisplayName] [nvarchar](256),
      [YearSemester] [nvarchar](256),
      [CalendarQuarterId] [int],
      [CalendarQuarterDisplayName] [nvarchar](256),
      [YearQuarter] [nvarchar](256),
      [MonthId] [int],
      [MonthDisplayName] [nvarchar](256),
      [YearMonth] [nvarchar](256),
      [CalendarWeekId] [int],
      [YearWeek] [nvarchar](256),
      [CalendarDateId] [int],
      [CalendarDayDisplayName] [nvarchar](256),
      [CalendarDayDate] [datetime2]
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/DimDate/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: DimDate';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'FactDataQuality'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[FactDataQuality]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: FactDataQuality';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[FactDataQuality]
(
    [FactDataQualityId] [int],
	[DQJobSourceId] [nvarchar](256),
	[DQRuleId] [int],
	[RuleScanCompletionDatetime] [datetime2](7),
	[DEHLastProcessedDatetime] [datetime2](7),
	[BusinessDomainId] [nvarchar](512),
	[DataProductId] [nvarchar](512),
	[DataAssetId] [nvarchar](512),
	[DataAssetColumnId] [nvarchar](512),
	[JobTypeId] [int],
	[DQRuleTypeId] [int],
	[DQScanProfileId] [int],
	[ScanCompletionDateId] [int],
	[DQOverallProfileQualityScore] [decimal](18, 10),
	[DQPassedCount] [int],
	[DQFailedCount] [int],
	[DQIgnoredCount] [int],
	[DQEmptyCount] [int],
	[DQMiscastCount] [int]
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/FactDataQuality/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: FactDataQuality';

IF EXISTS (
    SELECT *
    FROM sys.tables 
    WHERE [name] = 'FactDataGovernanceScan'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DimensionalSchema + '].[FactDataGovernanceScan]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: FactDataGovernanceScan';
END

SET @DynamicSQL = '
CREATE EXTERNAL TABLE [' + @DimensionalSchema + '].[FactDataGovernanceScan]
(
    [HealthControlId] [bigint],
	[DataProductId] [nvarchar](512),
	[DEHProcessingDateId] [int],
	[LastProcessedDatetime] [datetime2](7),
	[BusinessDomainId] [nvarchar](512),
	[TotalDataProductAssetCount] [int],
	[DataProductCounter] [int],
	[ClassifiedAssetCount] [int],
	[DataProductClassificationScore] [float],
	[DataProductIsClassified] [bigint],
	[LabeledAssetCount] [int],
	[DataProductLabelScore] [float],
	[DataProductIsLabeled] [bigint],
	[DataProductOwnerScore] [float],
	[DataProductHasOwner] [int],
	[DataProductDataAssetHasOwnerCount] [int],
	[DataProductObservabilityScore] [float],
	[DataProductAssetLinkageScore] [float],
	[DataProductAssetDQEnabledCount] [int],
	[DataProductAssetDQNotEnabledCount] [int],
	[DataProductAssetDQEnabledScore] [float],
	[DataProductCompositeDQScore] [float],
	[DataProductOKRLinkageScore] [float],
	[DataProductOKRLinkageCount] [int],
	[DataProductCriticalDataLinkageScore] [float],
	[DataProductCriticalDataLinkageCount] [int],
	[DataProductSelfServiceEnablementScore] [float],
	[DataProductSelfServiceAccessPolicyCount] [int],
	[DataProductSelfServiceDurationPolicyCount] [int],
	[DataProductSelfServiceSLAPolicyCount] [int],
	[DataProductComplianceDataTermsOfUsePolicyScore] [float],
	[DataProductComplianceDataTermsOfUsePolicyCount] [int],
	[DataProductPublishedCount] [int],
	[DataProductDraftCount] [int],
	[DataProductExpiredCount] [int],
	[DataProductHasDescription] [int],
	[DataProductHasAsset] [int],
	[DataProductHasPublishedGlossaryTerm] [int],
	[DataProductHasGlossaryTerm] [int],
	[DataProductHasUseCase] [int],
	[DataProductDataCatalogingScore] [float],
	[DataProductPublishedByAuthorizedUserCount] [int],
	[DataProductAssetMDQEnabledCount] [int],
	[DataProductAssetMDQNotEnabledCount] [int],
	[DataProductAssetMDQEnabledScore] [float],
	[DataProductCertifiedCount] [int],
	[DataProductAccessGrantCount] [int],
	[DataProductInSLAAccessGrantCount] [int],
	[DataProductAccessGrantScore] [float],
    [DataProductDescHasLenGr100Char] [int],
	[DataProductAssoBusinessDomainDescHasLenGr100Char] [int],
	[DataProductAssoAllTremDescHasLenGr25Char] [int],
	[DataProductUsabilityScore] [float],
	[DataProductHasParentBusinessDomain] [int],
	[DataProductHasAtLeast1Asset] [int],
	[DataProductHasAssoPubTerm] [int],
	[LinkedAssetsScore] [float],
	[DataProductAllAssoAssetsHasOwners] [int],
	[OwnershipScore] [float]
)
WITH (
    DATA_SOURCE = [@containerName],
    LOCATION = N''DimensionalModel/FactDataGovernanceScan/'',
    FILE_FORMAT = [DeltaLakeFormat]
)';
EXEC sp_executesql @DynamicSQL;
PRINT 'External table created: FactDataGovernanceScan';
PRINT 'DIMENSIONAL TABLES CREATION COMPLETE!'

PRINT 'CREATING VIEWs';

IF EXISTS (
    SELECT *
    FROM sys.views 
    WHERE [name] = 'vwFactDataGovernanceScan'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP VIEW [' + @DimensionalSchema + '].[vwFactDataGovernanceScan]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'VIEW dropped: vwFactDataGovernanceScan';
END

SET @DynamicSQL = '
CREATE VIEW [' + @DimensionalSchema + '].[vwFactDataGovernanceScan]
AS 
SELECT * 
FROM     
	(SELECT *, ROW_NUMBER() OVER (PARTITION BY BusinessDomainId, DataProductId, HealthControlId, CAST(LastProcessedDatetime AS DATE) 
	                              ORDER BY LastProcessedDatetime DESC) RID   
	FROM  ['+@DimensionalSchema+'].[FactDataGovernanceScan]) X  
WHERE X.RID=1';
EXEC sp_executesql @DynamicSQL;
PRINT 'VIEW created: vwFactDataGovernanceScan';

IF EXISTS (
    SELECT *
    FROM sys.views 
    WHERE [name] = 'vwFactDataQuality'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP VIEW [' + @DimensionalSchema + '].[vwFactDataQuality]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'VIEW dropped: vwFactDataQuality';
END

SET @DynamicSQL = '
CREATE VIEW [' + @DimensionalSchema + '].[vwFactDataQuality]
AS 
SELECT
F.DQJobSourceId
,F.DQRuleId
,F.RuleScanCompletionDatetime
,F.DEHLastProcessedDatetime
,F.BusinessDomainId
,F.DataProductId
,F.DataAssetId
,F.JobTypeId
,F.DQRuleTypeId
,F.DQScanProfileId
,F.ScanCompletionDateId
,F.DQOverallProfileQualityScore
,F.DQPassedCount
,F.DQFailedCount
,F.DQIgnoredCount
,F.DQEmptyCount
,F.DQMiscastCount
FROM [' + @DimensionalSchema + '].[FactDataQuality] F 
INNER JOIN [' + @DimensionalSchema + '].[DimDQScanProfile] D
ON F.DQScanProfileId = D.DQScanProfileId
WHERE D.RuleOriginDisplayName = ''Asset'' AND D.RuleAppliedOn = ''Asset''
UNION ALL
 
SELECT DISTINCT
F.DQJobSourceId
,F.DQRuleId
,F.RuleScanCompletionDatetime
,F.DEHLastProcessedDatetime
,F.BusinessDomainId
,F.DataProductId
,F.DataAssetId
,F.JobTypeId
,F.DQRuleTypeId
,F.DQScanProfileId
,F.ScanCompletionDateId
,F.DQOverallProfileQualityScore
,F.DQPassedCount
,F.DQFailedCount
,F.DQIgnoredCount
,F.DQEmptyCount
,F.DQMiscastCount
FROM [' + @DimensionalSchema + '].[FactDataQuality] F 
INNER JOIN [' + @DimensionalSchema + '].[DimDQScanProfile] D
ON F.DQScanProfileId = D.DQScanProfileId
WHERE D.RuleOriginDisplayName = ''Asset'' AND D.RuleAppliedOn = ''Column''
 
UNION ALL 
SELECT DISTINCT
F.DQJobSourceId
,F.DQRuleId
,F.RuleScanCompletionDatetime
,F.DEHLastProcessedDatetime
,F.BusinessDomainId
,F.DataProductId
,F.DataAssetId
,F.JobTypeId
,F.DQRuleTypeId
,F.DQScanProfileId
,F.ScanCompletionDateId
,F.DQOverallProfileQualityScore
,F.DQPassedCount
,F.DQFailedCount
,F.DQIgnoredCount
,F.DQEmptyCount
,F.DQMiscastCount
FROM [' + @DimensionalSchema + '].[FactDataQuality] F 
INNER JOIN [' + @DimensionalSchema + '].[DimDQScanProfile] D
ON F.DQScanProfileId = D.DQScanProfileId
WHERE D.RuleOriginDisplayName = ''CDE'' AND D.RuleAppliedOn = ''Column''
UNION ALL
SELECT F.DQJobSourceId
,F.DQRuleId
,F.RuleScanCompletionDatetime
,F.DEHLastProcessedDatetime
,F.BusinessDomainId
,F.DataProductId
,F.DataAssetId
,F.JobTypeId
,F.DQRuleTypeId
,F.DQScanProfileId
,F.ScanCompletionDateId
,F.DQOverallProfileQualityScore
,F.DQPassedCount
,F.DQFailedCount
,F.DQIgnoredCount
,F.DQEmptyCount
,F.DQMiscastCount
FROM [' + @DimensionalSchema + '].[FactDataQuality] F 
INNER JOIN [' + @DimensionalSchema + '].[DimDQScanProfile] D
ON F.DQScanProfileId = D.DQScanProfileId
WHERE D.RuleOriginDisplayName = ''CDE'' AND D.RuleAppliedOn = ''Asset''';
EXEC sp_executesql @DynamicSQL;
PRINT 'VIEW created: vwFactDataQuality';

IF EXISTS (
    SELECT *
    FROM sys.views 
    WHERE [name] = 'rptDataQuality'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DimensionalSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP VIEW [' + @DimensionalSchema + '].[rptDataQuality]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'VIEW dropped: rptDataQuality';
END

SET @DynamicSQL = '
CREATE VIEW [' + @DimensionalSchema + '].[rptDataQuality]
AS 
WITH CTE as
(
SELECT     a.BusinessDomainId,a.DataProductId,a.dataassetid,
           max(a.rulescancompletiondatetime) AS scantime 
from [' + @DimensionalSchema + '].[vwFactDataQuality] a
INNER JOIN (SELECT * FROM [' + @DimensionalSchema + '].[DimDQJobType] 
WHERE [JobTypeDisplayName] <> ''MDQ'')  b
on a.jobtypeid = b.jobtypeid
WHERE      b.[JobTypeDisplayName] <> ''MDQ''
GROUP BY   BusinessDomainId,DataProductId,dataassetid), 
CTE2 as
(
	 select a.* from
	 (
	 SELECT [DQJobSourceId]
		  ,[DQRuleId]
		  ,[RuleScanCompletionDatetime]
		  ,[DEHLastProcessedDatetime]
		  ,[BusinessDomainId]
		  ,[DataProductId]
		  ,[DataAssetId]
		  ,[JobTypeId]
		  ,[DQRuleTypeId]
		  ,[DQScanProfileId]
		  ,[ScanCompletionDateId]
		  ,max([DQOverallProfileQualityScore]) as [DQOverallProfileQualityScore]
		  ,max([DQPassedCount]) as [DQPassedCount]
		  ,max([DQFailedCount]) as [DQFailedCount]
		  ,max([DQIgnoredCount]) as [DQIgnoredCount]
		  ,max([DQEmptyCount]) as [DQEmptyCount]
		  ,max([DQMiscastCount]) as [DQMiscastCount]
	  FROM [' + @DimensionalSchema + '].[vwFactDataQuality]
	  GROUP BY
		   [DQJobSourceId]
		  ,[DQRuleId]
		  ,[RuleScanCompletionDatetime]
		  ,[DEHLastProcessedDatetime]
		  ,[BusinessDomainId]
		  ,[DataProductId]
		  ,[DataAssetId]
		  ,[JobTypeId]
		  ,[DQRuleTypeId]
		  ,[DQScanProfileId]
		  ,[ScanCompletionDateId]
		  ) a
	INNER JOIN [' + @DimensionalSchema + '].[DimDQJobType] b 
	on a.jobtypeid = b.jobtypeid
	INNER JOIN cte c on a.dataassetid = c.dataassetid 
	and a.BusinessDomainId = c.BusinessDomainId
	and a.DataProductId = c.DataProductId
	and c.scantime = a.rulescancompletiondatetime
	WHERE      b.[JobTypeDisplayName] <> ''MDQ'')
SELECT * FROM   CTE2';

EXEC sp_executesql @DynamicSQL;
PRINT 'VIEW created: rptDataQuality';

IF EXISTS (
    SELECT *
    FROM sys.views 
    WHERE [name] = 'vwDataAssetClassification'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP VIEW [' + @DomainSchema + '].[vwDataAssetClassification]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'VIEW dropped: vwDataAssetClassification';
END

SET @DynamicSQL = '
CREATE VIEW [' + @DomainSchema + '].[vwDataAssetClassification]
AS 
SELECT DISTINCT DP.DataProductID DataProductSourceId, DDP.DataProductID, DP.DataProductDisplayName, DA.DataAssetId AS DataAssetSourceId, DDA.DataAssetId, DA.AssetDisplayName, 
CASE WHEN DACC.DataAssetId IS NULL THEN 0 ELSE 1 END AS AssetClassificationFlag,DACC.ColumnId DataAssetColumnSourceId, 
DDAC.DataAssetColumnId,DACC.ColumnId, DAD.BusinessDomainId 
FROM [' + @DomainSchema + '].[DataProduct] DP 
INNER JOIN [' + @DimensionalSchema + '].[DimDataProduct]DDP 
	on DP.DataProductId=DDP.DataProductSourceId 
INNER JOIN [' + @DomainSchema + '].[DataProductAssetAssignment]DPA 
	on DPA.DataProductId=DP.DataProductID 
INNER JOIN [' + @DomainSchema + '].[DataAsset]DA 
	on DA.DataAssetId=DPA.DataAssetId  
INNER JOIN [' + @DimensionalSchema + '].[DimDataAsset]DDA 
	on DA.DataAssetId=DDA.DataAssetSourceId 
LEFT JOIN [' + @DomainSchema + '].[DataAssetColumnClassificationAssignment] DACC 
	ON DA.DataAssetId=DACC.DataAssetId 
LEFT JOIN [' + @DimensionalSchema + '].[DimDataAssetColumn] DDAC 
	ON DACC.ColumnId=DDAC.DataAssetColumnSourceId
LEFT JOIN [' + @DomainSchema + '].[DataAssetDomainAssignment] DAD ON
DA.DataAssetId = DAD.DataAssetId';
EXEC sp_executesql @DynamicSQL;
PRINT 'VIEW created: vwDataAssetClassification';
PRINT 'VIEW COMPLETE';

--COMMIT TRAN