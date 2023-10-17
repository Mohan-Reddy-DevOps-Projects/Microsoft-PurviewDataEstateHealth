USE [@databaseName];

IF NOT EXISTS(SELECT [name] FROM sys.external_data_sources WHERE [name] = '@containerName')
BEGIN
    CREATE EXTERNAL DATA SOURCE @containerName
    WITH (LOCATION = @containerUri, CREDENTIAL = @databaseScopedCredential);
END
GO

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'DeltaLakeFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT DeltaLakeFormat WITH (  FORMAT_TYPE = DELTA );
END
GO

IF NOT EXISTS(SELECT [name] FROM sys.external_file_formats WHERE [name] = 'ParquetFormat')
BEGIN
    CREATE EXTERNAL FILE FORMAT ParquetFormat WITH (  FORMAT_TYPE = PARQUET );
END
GO

BEGIN TRAN
    DECLARE @schemaId int = (select schema_id from sys.schemas where name = '@schemaName');

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAssetDefinition' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.DataAssetDefinition
    END

    BEGIN
        CREATE EXTERNAL TABLE @schemaName.DataAssetDefinition
        (
            DataAssetId NVARCHAR(255),
            CollectionId NVARCHAR(255), 
            DisplayName NVARCHAR(255),
            QualifiedName NVARCHAR(512), 
            SourceType NVARCHAR(255),
            ObjectType NVARCHAR(255),
            SourceInstance NVARCHAR(255),
            CurationLevel NVARCHAR(255),
            Provider NVARCHAR(255),
            Platform NVARCHAR(255),
            CreatedAt BIGINT,
            ModifiedAt BIGINT,
            CreatedBy NVARCHAR(255),
            ModifiedBy NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/DataAsset',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAssetAttributes' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.DataAssetAttributes
    END

    BEGIN
        CREATE EXTERNAL TABLE @schemaName.DataAssetAttributes
        (
            DataAssetId NVARCHAR(255),
            HasValidOwner BIT,
            HasNotNullDescription BIT,
            HasManualClassification BIT,
            UnclassificationReason NVARCHAR(512),
            HasSensitiveClassification BIT,
            HasSchema BIT,
            HasScannedClassification BIT,
            GlossaryTermCount INT
        )
        WITH (
            LOCATION= N'/AtlasRdd/DataAsset',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'Label' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.Label
    END

    BEGIN
        CREATE EXTERNAL TABLE @schemaName.Label
        (
            LinkedAssetId NVARCHAR(255),
            LinkedAssetType NVARCHAR(255),
            LabelId NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/Label',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END
    
    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAssetColumnDefinition' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.DataAssetColumnDefinition
    END
    
    BEGIN
        CREATE EXTERNAL TABLE @schemaName.DataAssetColumnDefinition
        (
            DataAssetId NVARCHAR(255),
            DataAssetColumnId NVARCHAR(255),
            DisplayName NVARCHAR(255),
			QualifiedName NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/DataAssetColumn',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'DataAssetColumnAttributes' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.DataAssetColumnAttributes
    END
    
    BEGIN
        CREATE EXTERNAL TABLE @schemaName.DataAssetColumnAttributes
        (
            DataAssetColumnId NVARCHAR(255),
			HasValidDescription BIT,
			HasUserDescription BIT,
			HasClassification BIT, 
			HasTerms BIT
        )
        WITH (
            LOCATION= N'/AtlasRdd/DataAssetColumn',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'ClassificationAssociation' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.ClassificationAssociation
    END

    BEGIN
        CREATE EXTERNAL TABLE @schemaName.ClassificationAssociation
        (
            DataAssetId NVARCHAR(255),
            DataAssetColumnId NVARCHAR(255),
            ClassificationId NVARCHAR(255),
			ClassificationSource NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/ClassificationAssociation',
            DATA_SOURCE = @configurationdataContainerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'Collection' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.Collection
    END
    
    BEGIN
        CREATE EXTERNAL TABLE @schemaName.Collection
        (
            CollectionId NVARCHAR(255),
            FriendlyPath NVARCHAR(255),
            CreatedAt BIGINT,
			ModifiedAt BIGINT,
			CreatedBy NVARCHAR(255),
			ModifiedBy NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/Collection',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END
    
    IF EXISTS(SELECT [name] FROM sys.tables WHERE [name] = 'Classification' AND schema_id = @schemaId)
    BEGIN
        DROP EXTERNAL TABLE @schemaName.Classification
    END
    
    BEGIN
        CREATE EXTERNAL TABLE @schemaName.Classification
        (
            Category NVARCHAR(255),
            ClassificationId NVARCHAR(255),
            ClassificationType NVARCHAR(255),
			Name NVARCHAR(255),
			DisplayName NVARCHAR(255),
			Description NVARCHAR(255)
        )
        WITH (
            LOCATION= N'/AtlasRdd/Classification',
            DATA_SOURCE = @containerName,
            FILE_FORMAT = DeltaLakeFormat
        )
    END

COMMIT TRAN
GO
