USE [@databaseName];

--BEGIN TRAN

DECLARE @DynamicSQL NVARCHAR(MAX);
DECLARE @DomainSchema NVARCHAR(512) = '@schemaName.DomainModel';
DECLARE @DimensionalSchema NVARCHAR(512) = '@schemaName.DimensionalModel';

PRINT 'DROPPING VIEWs';

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
PRINT 'VIEW DROP COMPLETE';

/*
DOMAIN MODEL SECTION
*/

PRINT 'START DROPPING OF DOMAIN EXTERNAL TABLES...'

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

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'CDE'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[CDE]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: CDE';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'CDEColumnAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[CDEColumnAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: CDEColumnAssignment';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'CDEDataProductAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[CDEDataProductAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: CDEDataProductAssignment';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'CDEGlossaryTermAssignment'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[CDEGlossaryTermAssignment]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: CDEGlossaryTermAssignment';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'Action'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[Action]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: Action';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'OKR'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[OKR]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: OKR';
END

IF EXISTS (
    SELECT * 
    FROM sys.tables 
    WHERE [name] = 'KeyResult'
    AND schema_id IN (SELECT schema_id FROM sys.schemas WHERE name = @DomainSchema)
)
BEGIN
    SET @DynamicSQL = 'DROP EXTERNAL TABLE [' + @DomainSchema + '].[KeyResult]';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'External table dropped: KeyResult';
END

PRINT 'DOMAIN EXTERNAL TABLES DROPS COMPLETE!'
/*
DIMENSIONAL MODEL SECTION
*/
PRINT 'STARTING DROPPING OF DIMENSIONAL EXTERNAL TABLES...'

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
PRINT 'DIMENSIONAL EXTERNAL TABLES DROPS COMPLETE!'

IF EXISTS (SELECT * FROM SYS.schemas WHERE name = @DomainSchema)
BEGIN
    SET @DynamicSQL = 'DROP SCHEMA [' + @DomainSchema + ']';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'Schema DROPPED: ' + @DomainSchema;
END

IF EXISTS (SELECT * FROM SYS.schemas WHERE name = @DimensionalSchema)
BEGIN
    SET @DynamicSQL = 'DROP SCHEMA [' + @DimensionalSchema + ']';
    EXEC sp_executesql @DynamicSQL;
    PRINT 'Schema DROPPED: ' + @DimensionalSchema;
END

--COMMIT TRAN
/* --Uncomment WHEN ENTIRE DB CLEAN UP IS NEEDED
PRINT 'External_data_sources DROPPED: @containerName';

IF EXISTS(SELECT [name] FROM sys.external_data_sources WHERE [name] = '@containerName')
BEGIN
    DROP EXTERNAL DATA SOURCE [@containerName];
END

PRINT 'External_file_formats DROPPED: DeltaLakeFormat';
IF EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'DeltaLakeFormat')
BEGIN
    DROP EXTERNAL FILE FORMAT DeltaLakeFormat;
END
PRINT 'External_file_formats DROPPED: ParquetFormat';
IF EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'ParquetFormat')
BEGIN
    DROP EXTERNAL FILE FORMAT ParquetFormat;
END
*/