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
            [BusinessDomainId] uniqueidentifier,
            [BusinessDomainDisplayName] nvarchar (256),
            [HasDescription] bit,
            [GlossaryTermCount] int,
            [DataProductCount] int,
            [IsRootDomain] bit,
            [HasValidOwner] bit,
            [CreatedAt] datetime2,
            [CreatedBy] nvarchar (64),
            [ModifiedAt] datetime2,
            [ModifiedBy] nvarchar (64),
            [LastRefreshedAt] datetime2
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
            [BusinessDomainId] uniqueidentifier,
            [ContactId] uniqueidentifier,
            [ContactRole] nvarchar (36),
            [IsActive] bit
        )
        WITH (
            LOCATION='/Sink/BusinessDomainContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = [DeltaLakeFormat]
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomainDataProductAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[BusinessDomainDataProductAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[BusinessDomainDataProductAssociation]
        (
            [DataProductId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier,
            [IsPrimaryDataProduct] bit
        )
        WITH (
            LOCATION='/Sink/DataProductDomainAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = [DeltaLakeFormat]
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'BusinessDomainTrends' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[BusinessDomainTrends]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[BusinessDomainTrends]
        (
            [BusinessDomainTrendId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier,
            [DataProductCount] int,
            [AssetCount] bigint,
            [TotalOpenActionsCount] int,
            [ClassificationPassCount] int,
            [GlossaryTermPassCount] int,
            [DataQualityScorePassCount] int,
            [DataProductDescriptionPassCount] int,
            [ValidDataProductOwnerPassCount] int,
            [DataShareAgreementSetOrExemptPassCount] int,
            [AuthoritativeSourcePassCount] int,
            [ValidUseCasePassCount] int,
            [ValidTermsOfUsePassCount] int,
            [AccessEntitlementPassCount] int,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION = 'Sink/BusinessDomainTrendsById/',
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
            [DataAssetId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier,
            [SourceAssetId] uniqueidentifier,
            [DisplayName] nvarchar (256),
            [ObjectType] nvarchar (256),
            [SourceType] nvarchar (256),
            [HasClassification] bit,
            [HasSchema] bit,
            [HasDescription] bit,
            [CreatedAt] datetime2,
            [CreatedBy] nvarchar(64),
            [ModifiedAt] datetime2,
            [ModifiedBy] nvarchar (64),
            [LastRefreshedAt] datetime2
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
            [DataAssetId] uniqueidentifier,
            [ContactRole] nvarchar (64),
            [ContactId] uniqueidentifier,
            [IsActive] bit
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
            [DataProductId] uniqueidentifier,
            [DataProductDisplayName] nvarchar (256),
            [DataProductType] nvarchar (64),
            [DataProductStatus] nvarchar (64),
            [HasValidOwner] bit,
            [HasValidUseCase] bit,
            [HasValidTermsOfUse] bit,
            [AssetCount] bigint,
            [HasDescription] bit,
            [IsAuthoritativeSource] bit,
            [HasDataQualityScore] bit,
            [ClassificationPassCount] int,
            [HasAccessEntitlement] bit,
            [HasDataShareAgreementSetOrExempt] bit,
            [GlossaryTermCount] int,
            [CreatedAt] datetime2,
            [CreatedBy] nvarchar (64),
            [ModifiedAt] datetime2,
            [ModifiedBy] nvarchar (64),
            [LastRefreshedAt] datetime2
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
            [DataProductId] uniqueidentifier,
            [ContactRole] nvarchar (36),
            [ContactId] uniqueidentifier,
            [IsActive] bit
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
            [DataAssetId] uniqueidentifier,
            [DataProductId] uniqueidentifier
        )
        WITH (
            LOCATION='/Sink/AssetProductAssociation/',
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
            [TermId] uniqueidentifier,
            [Name] nvarchar (256),
            [Status] nvarchar (64),
            [HasDescription] bit,
            [CreatedAt] datetime2,
	        [CreatedBy] nvarchar (64),
            [ModifiedAt] datetime2,
        	[ModifiedBy] nvarchar (64),
            [LastRefreshedAt] datetime2
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
            [TermId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier
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
            [TermId] uniqueidentifier,
            [ContactRole] nvarchar (36),
            [ContactId] uniqueidentifier,
            [IsActive] bit
        )
        WITH (
            LOCATION='/Sink/TermContactAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'ProductQualityScore' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[ProductQualityScore]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[ProductQualityScore]
        (
            [DataProductId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier,
            [QualityScore] float,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION='/Sink/ProductQualityScore/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'AssetQualityScore' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[AssetQualityScore]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[AssetQualityScore]
        (
            [DataProductId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier,
            [DataAssetId] uniqueidentifier,
            [QualityScore] float,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION='/Sink/AssetQualityScore/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DomainQualityScore' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DomainQualityScore]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DomainQualityScore]
        (
            [BusinessDomainId] uniqueidentifier,
            [QualityScore] float,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION='/Sink/DomainQualityScore/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END
COMMIT TRAN
