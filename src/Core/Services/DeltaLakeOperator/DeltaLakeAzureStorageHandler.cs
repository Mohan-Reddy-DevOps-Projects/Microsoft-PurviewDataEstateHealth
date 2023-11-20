// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using Microsoft.Data.DeltaLake.Storage;
using global::Azure.Storage.Files.DataLake;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// This interface is implemented as required by Delta Lake low-level library (Microsoft.Data.DeltaLake).
/// </summary>
public class DeltaLakeAzureStorageHandler : IStorage
{
    private readonly DataLakeFileSystemClient adlsGen2Container;
    private readonly EnvironmentConfiguration environmentConfig;

    /// <summary>
    /// DeltaLakeAzureStorageHandler C'tor.
    /// </summary>
    /// <param name="adlsGen2Container"></param>
    /// <param name="environmentConfig"></param>
    public DeltaLakeAzureStorageHandler(DataLakeFileSystemClient adlsGen2Container, EnvironmentConfiguration environmentConfig)
    {
        this.adlsGen2Container = adlsGen2Container;
        this.environmentConfig = environmentConfig;
    }

    /// <inheritdoc/>
    public string CombinePath(params string[] segments)
    {
        return string.Join('/', segments);
    }

    /// <summary>
    /// This returns the "folder path" of the adlsGen2. 
    /// </summary>
    public string GetDirectory(string path)
    {
        var adlsGen2Client = adlsGen2Container.GetDirectoryClient(path);
        string directoryPath = string.Join('/', adlsGen2Client.Uri.Segments.Take(adlsGen2Client.Uri.Segments.Length - 1));

        return directoryPath;
    }

    /// <summary>
    /// This function queries the Azure adlsGen2 Storage container for items that have the specified input path as a prefix.
    /// It returns the list of items as StorageItem objects, with only the file name and not the full path.
    /// </summary>
    public IEnumerable<StorageItem> GetFiles(string folderPath)
    {
        var items = new List<StorageItem>();

        // To convert the input path to "prefix-based" path for Azure storage query.
        string prefixPath = ConvertFolderPathToStorageSearchPrefix(folderPath);

        var folderClient = adlsGen2Container.GetDirectoryClient(folderPath);
        if (folderClient.Exists())
        {
            foreach (var adlsGen2Item in adlsGen2Container.GetPaths(folderPath))
            {
                // We need to return only the file name and not the path.
                var adlsGen2FileName = adlsGen2Item.Name.Substring(prefixPath.Length);
                items.Add(new StorageItem(adlsGen2FileName, prefixPath, adlsGen2Item.LastModified.UtcDateTime));
            }
        }

        return items;
    }

    /// <summary>
    /// Tries to create a path for the given partial path, and opens a writable stream to the file if successful.
    /// </summary>
    public bool TryCreatePath(string partialPath, bool allowOverwrite, out Stream file)
    {
        var adlsGen2Client = adlsGen2Container.GetFileClient(partialPath);
        if (!allowOverwrite && adlsGen2Client.Exists())
        {
            file = null;
            return false;
        }

        file = adlsGen2Client.OpenWrite(true);

        if (this.environmentConfig.IsDevelopmentEnvironment() && partialPath.Contains("/_delta_log/"))
        {
            // NOTE: This is needed so that "\r\n" line ending from Windows environment are replaced with "\n".
            // This ensures that Synapse can read this type of Delta Lake format files. We need to do this replacement only for metadata files under "/_delta_log/"
            file = new DevEnvironmentStream(file);
        }

        return true;
    }

    /// <inheritdoc/>
    public bool TryDeletePath(string partialPath)
    {
        var adlsGen2Client = adlsGen2Container.GetFileClient(partialPath);
        return adlsGen2Client.DeleteIfExists();
    }

    /// <summary>
    /// Tries to read the file from the given partial path, and returns a readable stream if successful.
    /// </summary>
    public bool TryReadPath(string partialPath, out Stream file)
    {
        var adlsGen2Client = adlsGen2Container.GetFileClient(partialPath);
        if (adlsGen2Client.Exists())
        {
            file = adlsGen2Client.OpenRead();
            return true;
        }

        file = null;
        return false;
    }

    private string ConvertFolderPathToStorageSearchPrefix(string folderPath)
    {
        return folderPath.TrimStart('/') + "/";
    }
}
