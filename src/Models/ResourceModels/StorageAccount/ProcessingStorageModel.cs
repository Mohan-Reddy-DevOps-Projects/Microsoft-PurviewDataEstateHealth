// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json;
using global::Azure.Core;

/// <summary>
/// Processing storage model.
/// </summary>
public class ProcessingStorageModel
{
    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the unique id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the properties.
    /// </summary>
    public ProcessingStoragePropertiesModel Properties { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the catalog identifier.
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// Processing storage model extensions.
/// </summary>
public static class ProcessingStorageModelExtensions
{
    /// <summary>
    /// Gets the processing storage account name.
    /// </summary>
    /// <param name="processingStorageModel"></param>
    /// <returns></returns>
    public static string GetStorageAccountName(this ProcessingStorageModel processingStorageModel)
    {
        string storageAccountId = processingStorageModel.Properties.ResourceId;
        ResourceIdentifier storageAccountResourceId = new(storageAccountId);
        
        return storageAccountResourceId.Name;
    }

    /// <summary>
    /// Gets the processing storage account dfs uri.
    /// </summary>
    /// <param name="processingStorageModel"></param>
    /// <returns></returns>
    public static string GetDfsEndpoint(this ProcessingStorageModel processingStorageModel)
    {
        return $"https://{processingStorageModel.GetStorageAccountName()}.{processingStorageModel.Properties.DnsZone}.dfs.{processingStorageModel.Properties.EndpointSuffix}";
    }
}
