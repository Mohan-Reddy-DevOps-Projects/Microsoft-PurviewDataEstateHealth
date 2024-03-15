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
            FILE_FORMAT = DeltaLakeFormat
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

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'ActionCenter' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[ActionCenter]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[ActionCenter]
        (
            [RowId] uniqueidentifier,
            [ActionId] nvarchar (512),
            [DisplayName] nvarchar (128),
            [Description] nvarchar (1024),
            [TargetType] nvarchar (128),
            [TargetId] uniqueidentifier,
            [TargetName] nvarchar (128),
            [OwnerContactId] nvarchar (36),
            [OwnerContactDisplayName] nvarchar (128),
            [HealthControlState] nvarchar (32),
            [HealthControlName] nvarchar (64),
            [HealthControlCategory] nvarchar (128),
            [ActionStatus] nvarchar (32),
            [BusinessDomainId] uniqueidentifier,
            [DataProductId] nvarchar (36),
            [DataAssetId] nvarchar (36),
            [CreatedAt] datetime2,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION = 'Sink/ActionCenter/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DomainHealthScores' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DomainHealthScores]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DomainHealthScores]
        (
            [BusinessDomainId] uniqueidentifier,
            [Description] nvarchar (1024),
            [Name] nvarchar (128),
            [ScoreKind] nvarchar (128),
            [ActualValue] float,
            [TotalDataProducts] int,
            [C2_Ownership] float,
            [C3_AuthoritativeSource] float,
            [C5_Catalog] float,
            [C6_Classification] float,
            [C7_Access] float,
            [C8_DataConsumptionPurpose] float,
            [C12_Quality] float,
            [DataHealth] float,
            [MetadataCompleteness] float,
            [Use] float,
            [Quality] float,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION = 'Sink/DomainHealthScores/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'HealthScores' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[HealthScores]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[HealthScores]
        (
            [RowId] uniqueidentifier,
            [Description] nvarchar (1024),
            [Name] nvarchar (128),
            [ScoreKind] nvarchar (128),
            [ActualValue] float,
            [TotalDataProducts] int,
            [C2_Ownership] float,
            [C3_AuthoritativeSource] float,
            [C5_Catalog] float,
            [C6_Classification] float,
            [C7_Access] float,
            [C8_DataConsumptionPurpose] float,
            [C12_Quality] float,
            [DataHealth] float,
            [MetadataCompleteness] float,
            [Use] float,
            [Quality] float,
            [LastRefreshedAt] datetime2
        )
        WITH (
            LOCATION = 'Sink/HealthScores/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DomainHealthSummary' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[DomainHealthSummary]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[DomainHealthSummary]
        (
            [BusinessDomainId] uniqueidentifier,
            [BusinessDomainDisplayName] nvarchar (128),
            [TotalBusinessDomains] int,
            [TotalDataProductsCount] int,
            [TotalCuratedDataAssetsCount] bigint,
            [TotalOpenActionsCount] bigint,
            [BusinessDomainsFilterListLink] nvarchar (512),
            [BusinessDomainsTrendLink] nvarchar (512),
            [DataProductsTrendLink] nvarchar (512),
            [DataAssetsTrendLink] nvarchar (512),
            [HealthActionsTrendLink] nvarchar (512),
            [TotalReportsCount] int,
            [ActiveReportsCount] int,
            [DraftReportsCount] int,
            [TotalCuratableDataAssetsCount] bigint,
            [TotalNonCuratableDataAssetsCount] bigint,
            [TotalCompletedActionsCount] int,
            [TotalDismissedActionsCount] int,
            [LastRefreshDate] datetime2
        )
        WITH (
            LOCATION = 'Sink/DomainHealthSummary/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'HealthSummary' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[HealthSummary]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[HealthSummary]
        (
            [RowId] uniqueidentifier,
            [TotalBusinessDomains] int,
            [TotalDataProductsCount] int,
            [TotalCuratedDataAssetsCount] bigint,
            [TotalOpenActionsCount] int,
            [BusinessDomainsFilterListLink] nvarchar (512),
            [BusinessDomainsTrendLink] nvarchar (512),
            [DataProductsTrendLink] nvarchar (512),
            [DataAssetsTrendLink] nvarchar (512),
            [HealthActionsTrendLink] nvarchar (512),
            [TotalReportsCount] int,
            [ActiveReportsCount] int,
            [DraftReportsCount] int,
            [TotalCuratableDataAssetsCount] bigint,
            [TotalNonCuratableDataAssetsCount] bigint,
            [TotalCompletedActionsCount] int,
            [TotalDismissedActionsCount] int,
            [LastRefreshDate] datetime2
        )
        WITH (
            LOCATION = 'Sink/HealthSummary/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'AssetDomainAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[AssetDomainAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[AssetDomainAssociation]
        (
            [DataAssetId] uniqueidentifier,
            [BusinessDomainId] uniqueidentifier
        )
        WITH (
            LOCATION='/Sink/AssetDomainAssociation/',
            DATA_SOURCE = [@containerName],
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'AssetProductAssociation' AND schema_id IN (select schema_id from sys.schemas where name = '@schemaName'))
    BEGIN
        DROP EXTERNAL TABLE [@schemaName].[AssetProductAssociation]
    END

    BEGIN
        CREATE EXTERNAL TABLE [@schemaName].[AssetProductAssociation]
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
