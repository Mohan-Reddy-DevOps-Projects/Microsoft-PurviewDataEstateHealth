// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Data.DeltaLake.Commands;
using Microsoft.Data.DeltaLake;
using Microsoft.Data.DeltaLake.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using Parquet.Serialization;
using global::Azure.Core;
using global::Azure.Storage.Files.DataLake;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// DeltaLakeOperator
/// </summary>
public class DeltaLakeOperator : IDeltaLakeOperator
{
    private const string Engine = "DeltaLakeStandalone/V1";

    private readonly DataLakeFileSystemClient adlsGen2Container;

    private readonly EnvironmentConfiguration environmentConfig;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    /// <summary>
    /// DeltaLakeOperator C'tor.
    /// </summary>
    /// <param name="adlsGen2Container"></param>
    /// <param name="environmentConfig"></param>
    /// <param name="dataEstateHealthRequestLogger"></param>
    public DeltaLakeOperator(
        DataLakeFileSystemClient adlsGen2Container,
        EnvironmentConfiguration environmentConfig,
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger)
    {
        this.adlsGen2Container = adlsGen2Container;
        this.environmentConfig = environmentConfig;
        this.dataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
    }

    /// <inheritdoc />
    public async Task CreateOrOverwriteDataset<T>(string tableRelativePath, StructType schema, IList<T> rows)
    {
        DeltaLog deltaLog = GetDeltaLog(adlsGen2Container, tableRelativePath);

        // Start a new OptimisticTransaction
        OptimisticTransaction transaction = deltaLog.StartTransaction();

        var filesToRemove = new List<RemoveFile>();
        if (deltaLog.Exists)
        {
            Snapshot snapshot = deltaLog.GetSnapshot();
            DeltaScan scan = new DeltaScan(snapshot);
            IEnumerable<AddFile> allFiles = scan.GetFiles();

            // Remove all the existing files as part of this transaction.
            filesToRemove = allFiles.Select(x => x.Remove()).ToList();
        }

        // All the transaction commands.
        var allCommands = new List<Command>();

        // Create a new metadata action
        Metadata metadata = new Metadata(Guid.NewGuid().ToString(), null, null, null, null, null, DateTime.UtcNow, schema);
        allCommands.Add(metadata);

        // Create a new protocol action
        Protocol protocol = new Protocol(1, 2);
        allCommands.Add(protocol);

        List<AddFile> addFiles = await WriteParquetFiles(deltaLog, rows);

        allCommands.AddRange(filesToRemove);
        allCommands.AddRange(addFiles);
        transaction.Commit(allCommands, OperationName.ReplaceTable, Engine);
    }

    /// <inheritdoc />
    public async Task CreateOrAppendDataset<T>(string tableRelativePath, StructType schema, IList<T> rows)
    {
        DeltaLog deltaLog = GetDeltaLog(adlsGen2Container, tableRelativePath);

        // Start a new OptimisticTransaction
        OptimisticTransaction transaction = deltaLog.StartTransaction();

        // All the transaction commands.
        var allCommands = new List<Command>();

        // Create a new metadata action
        Metadata metadata = new Metadata(Guid.NewGuid().ToString(), null, null, null, null, null, DateTime.UtcNow, schema);
        allCommands.Add(metadata);

        // Create a new protocol action
        Protocol protocol = new Protocol(1, 2);
        allCommands.Add(protocol);

        List<AddFile> addFiles = await WriteParquetFiles(deltaLog, rows);

        allCommands.AddRange(addFiles);
        transaction.Commit(allCommands, OperationName.Write, Engine);
    }

    /// <inheritdoc />
    public async Task<IList<T>> ReadDataset<T>(string tableRelativePath) where T : new()
    {
        DeltaLog deltaLog = GetDeltaLog(adlsGen2Container, tableRelativePath);
        Snapshot snapshot = deltaLog.GetSnapshot();
        DeltaScan scan = new DeltaScan(snapshot);
        IEnumerable<AddFile> allFiles = scan.GetFiles();

        List<T> rows = new List<T>();
        foreach (var fileMetadata in allFiles)
        {
            var path = deltaLog.Storage.CombinePath(deltaLog.RelativePath, fileMetadata.Path);
            if (deltaLog.Storage.TryReadPath(path, out var file))
            {
                using (file)
                {
                    IList<T> rowBatch = await ParquetSerializer.DeserializeAsync<T>(file);
                    rows.AddRange(rowBatch);
                }
            }
        }

        return rows;
    }

    private async Task<List<AddFile>> WriteParquetFiles<T>(DeltaLog deltaLog, IList<T> rows)
    {
        string parquetFileName = Guid.NewGuid().ToString() + ".parquet";
        string parquetFilePath = deltaLog.Storage.CombinePath(deltaLog.RelativePath, parquetFileName);

        if (!deltaLog.Storage.TryCreatePath(parquetFilePath, allowOverwrite: true, out var file))
        {
            throw new Exception($"Unable to create parquet file at {parquetFilePath}");
        }

        List<AddFile> filesToAdd = new List<AddFile>();
        using (file)
        {
            try
            {
                // Create the parquet file.
                await ParquetSerializer.SerializeAsync(rows, file);

                // Create an AddFile action for the new Parquet file
                AddFile addFile = new AddFile(parquetFileName, null, file.Position, DateTime.UtcNow);
                filesToAdd.Add(addFile);
            }
            catch (Exception exception)
            {
                this.dataEstateHealthRequestLogger.LogError($"Failed to save parquet file for {typeof(T)} at {parquetFileName}", exception);
            }
        }

        return filesToAdd;
    }

    private DeltaLog GetDeltaLog(DataLakeFileSystemClient adlsGen2Container, string tableRelativePath)
    {
        var storage = new DeltaLakeAzureStorageHandler(adlsGen2Container, environmentConfig);

        string formattedRelativePath = FormatTableRelativePath(tableRelativePath);
        return DeltaLog.ForTable(new DeltaConfiguration(), storage, formattedRelativePath);
    }

    private string FormatTableRelativePath(string tableRelativePath)
    {
        if (tableRelativePath.StartsWith("/"))
        {
            return tableRelativePath;
        }
        else
        {
            return "/" + tableRelativePath;
        }
    }
}
