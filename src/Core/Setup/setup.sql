USE [@databaseName];

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'ParquetFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT ParquetFormat WITH (  FORMAT_TYPE = PARQUET );
END

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'DeltaLakeFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT DeltaLakeFormat WITH (  FORMAT_TYPE = DELTA );
END

IF NOT EXISTS(SELECT [name] FROM sys.external_data_sources WHERE [name] = '@containerName')
BEGIN
    CREATE EXTERNAL DATA SOURCE [@containerName]
    WITH (LOCATION = @containerUri, CREDENTIAL = @databaseScopedCredential);
END

BEGIN TRAN
DECLARE @DomianSchema NVARCHAR(512) = [@schemaName]+'.DomainModel';
IF NOT EXISTS ( SELECT * FROM SYS.schemas WHERE name = @DomianSchema )
BEGIN 
	EXEC ('CREATE SCHEMA ['+@DomianSchema+']')
END

DECLARE @DimensionSchema NVARCHAR(512) = [@schemaName]+'.DimensionalModel';
IF NOT EXISTS ( SELECT * FROM SYS.schemas WHERE name = @DimensionSchema )
BEGIN 
	EXEC ('CREATE SCHEMA ['+@DimensionSchema+']')
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'AccessPolicyResourceType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[AccessPolicyResourceType]
END

BEGIN 
CREATE EXTERNAL TABLE [@DomianSchema].[AccessPolicyResourceType]
(
	 [ResourceTypeId] [uniqueidentifier],
     [ResourceTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/AccessPolicyResourceType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'AccessPolicyProvisioningState'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[AccessPolicyProvisioningState]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[AccessPolicyProvisioningState]
(
	 [ProvisioningStateId] [nvarchar](256),
     [ProvisioningStateDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/AccessPolicyProvisioningState/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'Relationship'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[Relationship]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[Relationship]
(
	[AccountId] [uniqueidentifier],
	[Type] [nvarchar](512),
	[SourceType] [nvarchar](512),
	[SourceId] [uniqueidentifier],
	[TargetType] [nvarchar](512),
	[TargetId] [uniqueidentifier],
	[ModifiedByUserId] [uniqueidentifier],
	[ModifiedDateTime] [datetime2](7),
	[EventProcessingTime] [bigint],
	[OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/Relationship/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'GlossaryTermBusinessDomainAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[GlossaryTermBusinessDomainAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[GlossaryTermBusinessDomainAssignment]
(
	   [GlossaryTermID] [uniqueidentifier],
       [BusinessDomainId] [uniqueidentifier],
       [AssignedByUserId]  [uniqueidentifier],
       [AssignmentDatetime]  [datetime2],
       [GlossaryTermStatus] [nvarchar](512),
       [ActiveFlag] [int],
       [ActiveFlagLastModifiedDatetime]  [datetime2],
       [CreatedDatetime]  [datetime2],
       [CreatedByUserId]  [uniqueidentifier],
       [ModifiedDateTime]  [datetime2],
       [ModifiedByUserId]  [uniqueidentifier],
       [EventProcessingTime] [bigint],
	   [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/GlossaryTermBusinessDomainAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'GlossaryTermDataProductAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[GlossaryTermDataProductAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[GlossaryTermDataProductAssignment]
(
	  [GlossaryTermID] [uniqueidentifier],
	  [DataProductId] [uniqueidentifier],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/GlossaryTermDataProductAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'GlossaryTerm'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[GlossaryTerm]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[GlossaryTerm]
(
	 [GlossaryTermId] [uniqueidentifier],
     [ParentGlossaryTermId] [uniqueidentifier],
     [GlossaryTermDisplayName] [nvarchar](512),
     [GlossaryDescription] [nvarchar](512),
     [AccountId] [uniqueidentifier],
     [Status] [nvarchar](50),
     [IsLeaf] [Int],
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [uniqueidentifier],
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [uniqueidentifier],
     [EventProcessingTime] [bigint],
	 [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/GlossaryTerm/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityRuleType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityRuleType]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityRuleType]
(
	 [RuleTypeId] [nvarchar](256),
     [RuleTypeDisplayName] [nvarchar](512),
     [RuleTypeDesc] [nvarchar](512),
     [DimensionDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityRuleType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityRuleColumnExecution'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityRuleColumnExecution]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityRuleColumnExecution]
(
	 [JobExecutionId] [nvarchar](256),
     [RuleId] [nvarchar](256),
     [DataAssetId] [uniqueidentifier],
     [ColumnId] [nvarchar](256),
     [ColumnResultScore] [decimal](18,10),
     [RowPassCount] [Int],
     [RowFailCount] [Int],
     [RowMiscastCount] [Int],
     [RowEmptyCount] [Int],
     [RowIgnoredCount] [Int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityRuleColumnExecution/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityRule'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityRule]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityRule]
(
	  [RuleId] [nvarchar](256),
      [SourceRuleId] [nvarchar](256),
      [BusinessDomainId] [uniqueidentifier],--[nvarchar](256),
      [DataProductId] [uniqueidentifier],--[nvarchar](256),
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityRule/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityJobExecution'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityJobExecution]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityJobExecution]
(
	 [JobExecutionId] [nvarchar](256),
     [JobExecutionStatusDisplayName] [nvarchar](512),
     [JobType] [nvarchar](512),
     [ScanTypeDisplayName] [nvarchar](512),
     [JobCreationDatetime] [datetime2],
     [ExecutionStartDatetime] [datetime2],
     [ExecutionEndDatetime] [datetime2],
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityJobExecution/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityAssetRuleExecution'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityAssetRuleExecution]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityAssetRuleExecution]
(
	 [JobExecutionId] [nvarchar](256),
     [RuleId] [nvarchar](256),
     [DataAssetId] [uniqueidentifier],
     [AssetResultScore] [decimal](18,10),
     [RowPassCount] [Int],
     [RowFailCount] [Int],
     [RowMiscastCount] [Int],
     [RowEmptyCount] [Int],
     [RowIgnoredCount] [Int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityAssetRuleExecution/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductUpdateFrequency'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductUpdateFrequency]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductUpdateFrequency]
(
	 [UpdateFrequencyId] [uniqueidentifier],
     [UpdateFrequencyDisplayName] [nvarchar](512),
     [SortOrder] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductUpdateFrequency/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductType]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductType]
(
	 [DataProductTypeID] [nvarchar](256),
     [DataProductTypeDisplayName] [nvarchar](512),
     [DataProductTypeDescription] [nvarchar](max)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductTermsOfUse'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductTermsOfUse]
END

BEGIN 
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductTermsOfUse]
(
	 [DataProductId] [uniqueidentifier],
     [TermsOfUseId] [nvarchar](256),
     [TermsOfUseDisplayName] [nvarchar](512),
     [TermsOfUseHyperlink] [nvarchar](512),
     [DataAssetId] [uniqueidentifier]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductTermsOfUse/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductStatus'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductStatus]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductStatus]
(
	 [DataProductStatusID] [nvarchar](256),
     [DataProductStatusDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductStatus/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductDocumentation'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductDocumentation]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductDocumentation]
(
	 [DataProductId] [uniqueidentifier],
     [DocumentationId] [nvarchar](256),
     [DocumentationDisplayName] [nvarchar](512),
     [DocumentationHyperlink] [nvarchar](512),
     [DataAssetId] [uniqueidentifier]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductDocumentation/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductBusinessDomainAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductBusinessDomainAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductBusinessDomainAssignment]
(
	 [DataProductID] [uniqueidentifier],
     [BusinessDomainId] [uniqueidentifier],
     [AssignedByUserId] [uniqueidentifier],
     [AssignmentDateTime] [datetime2],
     [ActiveFlag] [int],
     [ActiveFlagLastModifiedDateTime] [datetime2]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductBusinessDomainAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductAssetAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductAssetAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductAssetAssignment]
(
	  [DataProductId] [uniqueidentifier],
	  [DataAssetId] [uniqueidentifier],
      [AssignedByUserId] [uniqueidentifier],
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductAssetAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END	

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProductOwner'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProductOwner]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProductOwner]
(
	 [DataProductId] [uniqueidentifier],
	 [DataProductOwnerId] [uniqueidentifier],
     [DataProductOwnerDescription] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProductOwner/',FILE_FORMAT = [DeltaLakeFormat])
END	

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataProduct'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataProduct]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataProduct]
(
	  [DataProductID] [uniqueidentifier],
      [DataProductDisplayName] [nvarchar](512),
      [DataProductDescription] [nvarchar](max),
      [AccountId] [uniqueidentifier],
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
      [UpdateFrequencyId] [uniqueidentifier],
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [uniqueidentifier],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataProduct/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetTypeDataType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetTypeDataType]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetTypeDataType]
(
	 [DataTypeId] [nvarchar] (256),
	 [DataAssetTypeId] [nvarchar] (256),
     [DataTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetTypeDataType/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetType]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetType]
(
	 [DataAssetTypeId] [nvarchar](256),
     [DataAssetTypeDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetType/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetOwnerAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetOwnerAssignment]
END

BEGIN 
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetOwnerAssignment]
(
	  [DataAssetId] [uniqueidentifier],
	  [DataAssetOwnerId] [nvarchar](256),
      [AssignedByUserId] [uniqueidentifier],	  
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetOwnerAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetOwner'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetOwner]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetOwner]
(
	 [DataAssetOwnerId] [nvarchar](256),
     [DataAssetOwner] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetOwner/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetDomainAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetDomainAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetDomainAssignment]
(
	  [DataAssetId] [uniqueidentifier],
	  [BusinessDomainId] [uniqueidentifier],
      [AssignedByUserId] [uniqueidentifier],	  
      [ActiveFlagLastModifiedDatetime] [datetime2],
      [AssignmentLastModifiedDatetime] [datetime2],
      [ActiveFlag] [int],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetDomainAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetColumnClassificationAssignment'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetColumnClassificationAssignment]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetColumnClassificationAssignment]
(
	 [DataAssetId] [nvarchar] (256),
	 [ColumnId] [nvarchar] (256),
	 [ClassificationId] [nvarchar] (256),
     [ModifiedDateTime] [datetime2]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetColumnClassificationAssignment/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAssetColumn'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAssetColumn]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataAssetColumn]
(
	  [DataAssetId] [uniqueidentifier],
      [ColumnId] [nvarchar](256),
      [ColumnDisplayName] [nvarchar](512),
      [ColumnDescription] [nvarchar](512),
      [DataAssetTypeId] [nvarchar](256),
      [DataTypeId] [nvarchar](256),
      [AccountId] [nvarchar](256),
      [CreatedDatetime] [datetime2],
      [CreatedByUserId] [uniqueidentifier],
      [ModifiedDateTime] [datetime2],
      [ModifiedByUserId] [uniqueidentifier],
      [EventProcessingTime] [bigint],
	  [OperationType] [nvarchar](50),
      [ClassificationId] [uniqueidentifier],
      [ColumnDataType] [nvarchar](50),
      [ColumnClassification] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAssetColumn/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataAsset'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataAsset]
END

BEGIN 
CREATE EXTERNAL TABLE [@DomianSchema].[DataAsset]
(
	  [DataAssetId] [uniqueidentifier],
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataAsset/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataQualityRuleType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataQualityRuleType]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataQualityRuleType]
(
	 [AccessPolicySetId] [uniqueidentifier],
     [AccessUseCaseDisplayName] [nvarchar](512),
     [AccessUseCaseDescription] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataQualityRuleType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'Classification'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[Classification]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[Classification]
(
	 [ClassificationId] [nvarchar] (256),
	 [ClassificationDisplayName] [nvarchar] (256),
	 [ClassificationDescription] [nvarchar] (256)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/Classification/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'BusinessDomain'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[BusinessDomain]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[BusinessDomain]
(
	 [BusinessDomainId] [nvarchar](256),
     [ParentBusinessDomainId] [uniqueidentifier],
     [BusinessDomainName] [nvarchar](512),
     [BusinessDomainDisplayName] [nvarchar](512),
     [BusinessDomainDescription] [nvarchar](max),
     [Status] [nvarchar](50),
     [IsRootDomain] [bit],
     [HasValidOwner] [bit],
     [AccountId] [uniqueidentifier],
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [uniqueidentifier],
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [uniqueidentifier],
     [EventProcessingTime] [bigint],
	 [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/BusinessDomain/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DataSubscriberRequest'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[DataSubscriberRequest]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[DataSubscriberRequest]
(
     [SubscriberRequestId] [uniqueidentifier],
     [DataProductId] [uniqueidentifier],
     [BusinessDomainId] [uniqueidentifier],
     [AccessPolicySetId] [uniqueidentifier],
     [SubscriberIdentityTypeDisplayName] [nvarchar](512),
     [RequestorIdentityTypeDisplayName] [nvarchar](512),
     [SubscriberRequestStatus] [nvarchar](512),
     [RequestStatusDisplayName] [nvarchar](512),
     [SubscribedByUserId] [uniqueidentifier],
     [SubscribedByUserTenantId] [uniqueidentifier],
     [SubscribedByUserEmail] [nvarchar](512),
     [RequestedByUserId] [uniqueidentifier],
     [RequestedByUserTenantId] [uniqueidentifier],
     [RequestedByUserEmail] [nvarchar](512),
     [RequestWriteAccess] [int] ,
     [RequestAccessDecisionDateTime] [datetime2],
     [Version] [nvarchar](512),
     [AccountId] [uniqueidentifier],
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](256),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](256),
     [EventProcessingTime] [bigint],
     [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/DataSubscriberRequest/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'PolicySetApprover'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[PolicySetApprover]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[PolicySetApprover]
(
     [SubscriberRequestId] [uniqueidentifier],
     [AccessPolicySetId] [uniqueidentifier],
     [ApproverUserId] [uniqueidentifier],
     [ApproverIdentityType] [nvarchar](512),
     [ApproverUserTenantId] [uniqueidentifier],
     [ApproverUserEmail] [nvarchar](512),
     [TermsOfUseRequired] [int] ,
     [PartnerExposurePermitted] [int] ,
     [CustomerExposurePermitted] [int] ,
     [PrivacyComplianceApprovalRequired] [int] ,
     [ManagerApprovalRequired] [int],
     [DataCopyPermitted] [int],
     [AccountId] [uniqueidentifier],
     [CreatedDatetime] [datetime2],
     [CreatedByUserId] [nvarchar](256),
     [ModifiedDateTime] [datetime2],
     [ModifiedByUserId] [nvarchar](256),
     [EventProcessingTime] [bigint],
     [OperationType] [nvarchar](50)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/PolicySetApprover/',FILE_FORMAT = [DeltaLakeFormat])
END




IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'AccessPolicySet'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DomianSchema'))
BEGIN DROP EXTERNAL  TABLE [@DomianSchema].[AccessPolicySet]
END

BEGIN  
CREATE EXTERNAL TABLE [@DomianSchema].[AccessPolicySet]
(
	  [AccessPolicySetId] [uniqueidentifier],
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
      [PolicyAppliedOnId] [uniqueidentifier],
      [PolicyAppliedOn] [nvarchar](256),
      [AccessPolicySetVersion] [int],
      [AccountId] [uniqueidentifier],
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DomainModel/AccessPolicySet/',FILE_FORMAT = [DeltaLakeFormat])
END


IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimBusinessDomain'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN   
DROP EXTERNAL TABLE [@DimensionalSchema].[DimBusinessDomain]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimBusinessDomain]
(
	 [BusinessDomainId] [int],
     [BusinessDomainSourceId] [nvarchar](256),
     [BusinessDomainDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimBusinessDomain/',FILE_FORMAT = [DeltaLakeFormat])
END

BEGIN
IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDataProduct'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDataProduct]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDataProduct]
(
	[DataProductId] [int],
	[DataProductSourceId] [nvarchar](256),
	[DataProductDisplayName] [nvarchar](512),
	[DataProductStatus] [nvarchar](50),
	[CreatedDatetime] [datetime2](7),
	[ModifiedDatetime] [datetime2](7),
	[IsActive] [bit]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDataProduct/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDataAsset'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDataAsset]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDataAsset]
(
	 [DataAssetId] [int],
     [DataAssetSourceId] [nvarchar](256),
     [DataAssetDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDataAsset/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDataAssetColumn'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDataAssetColumn]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDataAssetColumn]
(
	 [DataAssetColumnId] [int],
     [DataAssetColumnSourceId] [nvarchar](256),
     [DataAssetColumnDisplayName] [nvarchar](512),
     [CreatedDatetime] [datetime2],
     [ModifiedDatetime] [datetime2],
     [EffectiveDateTime] [datetime2],
     [ExpiredDateTime] [datetime2],
     [CurrentIndicator] [int]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDataAssetColumn/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDQScanProfile'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDQScanProfile]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDQScanProfile]
(
	 [DQScanProfileId] [Int],
     [RuleOriginDisplayName] [nvarchar](512),
     [RuleAppliedOn] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDQScanProfile/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDQRuleType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDQRuleType]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDQRuleType]
(
	 [DQRuleTypeId] [int],
     [DQRuleTypeSourceId] [nvarchar](256),
     [DQRuleTypeDisplayName] [nvarchar](512),
     [QualityDimension] [nvarchar](512),
     [QualityDimensionCustomIndicator] [nvarchar](256)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDQRuleType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDQRuleName'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDQRuleName]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDQRuleName]
(
	 [DQRuleId] [Int],
     [DQRuleNameId] [nvarchar](256),
     [DQRuleNameSourceId] [nvarchar](256),
     [DQRuleNameDisplayName] [nvarchar](512)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDQRuleName/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDataHealthControl'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDataHealthControl]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDataHealthControl]
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDataHealthControl/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDQJobType'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDQJobType]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDQJobType]
(
	[JobTypeId] [int],
	[JobTypeDisplayName] [nvarchar](256)
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDQJobType/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'DimDate'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[DimDate]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[DimDate]
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/DimDate/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'FactDataQuality'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[FactDataQuality]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[FactDataQuality]
(
	[FactDataQualityId] [int],
	[DQJobSourceId] [nvarchar](256),
	[DQRuleId] [int],
	[RuleScanCompletionDatetime] [datetime2](7),
	[DEHLastProcessedDatetime] [datetime2](7),
	[BusinessDomainId] [int],
	[DataProductId] [int],
	[DataAssetId] [int],
	[DataAssetColumnId] [int],
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
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/FactDataQuality/',FILE_FORMAT = [DeltaLakeFormat])
END

IF  EXISTS 
	(SELECT * 
	 FROM sys.tables 
	 WHERE [name] = 'FactDataGovernanceScan'
		AND schema_id IN (select schema_id from sys.schemas where name = '@DimensionalSchema'))
BEGIN
DROP EXTERNAL TABLE [@DimensionalSchema].[FactDataGovernanceScan]
END

BEGIN
CREATE EXTERNAL TABLE [@DimensionalSchema].[FactDataGovernanceScan]
(
	[HealthControlId] [bigint],
	[DataProductId] [bigint],
	[DEHProcessingDateId] [int],
	[LastProcessedDatetime] [datetime2](7),
	[BusinessDomainId] [bigint],
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
	[DataProductAccessGrantScore] [float]
)
WITH (DATA_SOURCE = [@containerName],LOCATION = N'DimensionalModel/FactDataGovernanceScan/',FILE_FORMAT = [DeltaLakeFormat])
END

COMMIT TRAN
