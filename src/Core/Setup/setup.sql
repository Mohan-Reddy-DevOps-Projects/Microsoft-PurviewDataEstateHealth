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
    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomain' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[BusinessDomain]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[BusinessDomain]
        (
            [BusinessDomainId] nvarchar (64),
            [BusinessDomainDisplayName] nvarchar (256),
            [HasNotNullDescription] int,
            [CreatedAt] nvarchar (32),
            [CreatedBy] nvarchar (256),
            [ModifiedAt] nvarchar (32),
            [ModifiedBy] nvarchar (256),
            [GlossaryTermCount] int,
            [DataProductCount] int,
            [IsRootDomain] int,
            [LastRefreshedAt] int,
            [HasValidOwner] int
        )
        WITH (
            LOCATION='/Sink/BusinessDomainSchema/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = [DeltaLakeFormat]
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomainContactAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[BusinessDomainContactAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[BusinessDomainContactAssociation]
        (
            [BusinessDomainId] nvarchar (64),
            [ContactId] nvarchar (64),
            [ContactRole] nvarchar (64),
            [IsActive] int
        )
        WITH (
            LOCATION='/Sink/BusinessDomainContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = [DeltaLakeFormat]
        )
    END

    -- IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomainDataProductAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    -- BEGIN
    --     DROP EXTERNAL TABLE [@schemaName].[BusinessDomainDataProductAssociation]
    -- END

    -- BEGIN
    --     CREATE EXTERNAL TABLE [@schemaName].[BusinessDomainDataProductAssociation]
    --     (
    --         [DataProductId] nvarchar (64),
    --         [BusinessDomainId] nvarchar (64),
    --         [IsPrimaryDataProduct] int
    --     )
    --     WITH (
    --         LOCATION='/Sink/DataProductDomainAssociation/',
    --         DATA_SOURCE = [@containerName],
    --         FILE_FORMAT = [DeltaLakeFormat]
    --     )
    -- END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomainTrends' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[BusinessDomainTrends]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[BusinessDomainTrends]
        (
            [BusinessDomainTrendId] nvarchar (64),
            [BusinessDomainId] nvarchar (64),
            [DataProductCount] int,
            [AssetCount] int,
            [ClassificationPassCount] int,
            [GlossaryTermPassCount] int,
            [DataQualityScorePassCount] int,
            [NotNullDataProductDescriptionPassCount] int,
            [ValidDataProductOwnerPassCount] int,
            [DataShareAgreementSetOrExemptPassCount] int,
            [AuthoratativeSourcePassCount] int,
            [ValidUseCasePassCount] int,
            [ValidTermsOfUsePassCount] int,
            [AccessEntitlementPassCount] int,
            [LastRefreshedAt] int
        )
        WITH (
            LOCATION = 'Sink/BusinessDomainTrends/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = [ParquetFormat]
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAsset' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DataAsset]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DataAsset]
        (
            [DataAssetId] nvarchar (64),
            [_IGNORE_] nvarchar (64),
            [__IGNORE__] nvarchar (64),
            [CreatedAt] nvarchar (128),
            [CreatedBy] nvarchar (256),
            [ModifiedBy] nvarchar (256),
            [ModifiedAt] nvarchar (32),
            [LastRefreshedAt] int,
            [HasNotNullDescription] int,
            [CollectionId] nvarchar (64),
            [DataAssetDisplayName] nvarchar(256),
            [QualifiedName] nvarchar(512),
            [SourceType] nvarchar (64),
            [ObjectType] nvarchar (64),
            [SourceInstance] nvarchar (64),
            [CurationLevel] nvarchar (64),
            [Provider] nvarchar (64),
            [Platform] nvarchar (64),
            [HasValidOwner] int,
            [HasManualClassification] int,
            [UnclassificationReason] nvarchar(512),
            [HasSensitiveClassification] int,
            [HasSchema] int,
            [HasScannedClassification] int,
            [GlossaryTermCount] int
        )
        WITH (
            LOCATION='/Sink/DataAssetSchema/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAssetContactAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DataAssetContactAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DataAssetContactAssociation]
        (
            [DataAssetId] nvarchar (64),
            [ContactRole] nvarchar (64),
            [ContactId] nvarchar (64),
            [IsActive] int
        )
        WITH (
            LOCATION='/Sink/DataAssetContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataProduct' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DataProduct]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DataProduct]
        (
            [DataProductId] nvarchar (64),
            [DataProductDisplayName] nvarchar (256),
            [DataProductType] nvarchar (32),
            [HasValidOwner] int,
            [HasValidUseCase] int,
            [HasValidTermsofUse] int,
            [DataPoductStatus] nvarchar (32),
            [AssetCount] nvarchar (8), -- BUG BUG
            [CreatedAt] nvarchar (32),
            [HasNotNullDescription] int,
            [CreatedBy] nvarchar (256),
            [ModifiedAt] nvarchar (32),
            [ModifiedBy] nvarchar (256),
            [LastRefreshedAt] int,
            [IsAuthoritativeSource] int,
            [HasDataQualityScore] int,
            [ClassificationPassCount] int,
            [HasAccessEntitlement] int,
            [HasDataShareAgreementSetOrExempt] int,
            [GlossaryTermCount] int
        )
        WITH (
            LOCATION='/Sink/DataProductSchema/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataProductContactAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DataProductContactAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DataProductContactAssociation]
        (
            [DataProductId] nvarchar (64),
            [ContactRole] nvarchar (64),
            [ContactId] nvarchar (64),
            [IsActive] int
        )
        WITH (
            LOCATION='/Sink/DataProductContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataProductDataAssetAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DataProductDataAssetAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DataProductDataAssetAssociation]
        (
            [_IGNORE_] nvarchar (64),
            [DataProductId] nvarchar (64),
            [DataAssetId] nvarchar (64)
        )
        WITH (
            LOCATION='/Sink/AssetDomainProductAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'GlossaryTerm' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[GlossaryTerm]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[GlossaryTerm]
        (
            [TermId] nvarchar (64),
            [Name] nvarchar (256),
            [HasFullDescription] bigint,
            [CreatedAt] nvarchar (32),
            [ModifiedAt] nvarchar (32),
            [Status] nvarchar (32),
            [LastRefreshedAt] bigint,
            [NickName] nvarchar(256),
	        [CreatedBy] nvarchar(256),
        	[ModifiedBy] nvarchar(256)
        )
        WITH (
            LOCATION='/Sink/TermSchema/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'GlossaryTermBusinessDomainAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[GlossaryTermBusinessDomainAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[GlossaryTermBusinessDomainAssociation]
        (
            [BusinessDomainId] nvarchar (64),
            [TermId] nvarchar (64)
        )
        WITH (
            LOCATION='/Sink/TermDomainAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'GlossaryTermContactAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[GlossaryTermContactAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[GlossaryTermContactAssociation]
        (
            [TermId] nvarchar (64),
            [ContactRole] nvarchar (64),
            [ContactId] nvarchar (64),
            [IsActive] int
        )
        WITH (
            LOCATION='/Sink/TermContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END
COMMIT TRAN
