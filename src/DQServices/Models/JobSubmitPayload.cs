namespace Microsoft.Purview.DataEstateHealth.DHModels.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Newtonsoft.Json;
using System.Collections.Generic;

public class JobSubmitPayload
{
    public JobSubmitPayload(
        // e.g. https://dgprocessingwus2cyqgjoc.z28.blob.storage.azure.net
        string storageEndpoint,
        string catalogId,
        string dataProductId,
        string dataAssetId,
        string jobId)
    {
        this.DatasetToDatasourceMappings = new List<DatasetToDatasourceMapping>
        {
            new DatasetToDatasourceMapping()
        };

        this.Error = new ErrorOutputInfo(
            storageEndpoint,
            catalogId,
            dataProductId,
            dataAssetId,
            jobId);
    }

    [JsonProperty("datasetToDatasourceMappings")]
    public List<DatasetToDatasourceMapping> DatasetToDatasourceMappings { get; set; }

    [JsonProperty("error")]
    public ErrorOutputInfo Error { get; set; }
}

public class DatasetToDatasourceMapping
{
    [JsonProperty("datasetAliasName")]
    public string DatasetAliasName => "primary";

    [JsonProperty("datasourceId")]
    public string DatasourceId => DataEstateHealthConstants.DEH_DATA_SOURCE_ID;
}

public class ErrorOutputInfo
{
    public ErrorOutputInfo(
        // e.g. https://dgprocessingwus2cyqgjoc.z28.blob.storage.azure.net
        string storageEndpoint,
        string catalogId,
        string dataProductId,
        string dataAssetId,
        string jobId)
    {
        var tmp = storageEndpoint.Substring("https://".Length);
        var splited = tmp.Split(".");
        this.Account = splited[0];
        this.DnsZone = splited[1];

        this.FileSystem = catalogId;
        this.FolderPath = $"all-errors/businessDomain={DataEstateHealthConstants.DEH_DOMAIN_ID}/dataProduct={dataProductId}/dataAsset={dataAssetId}/observation={jobId}";
    }

    [JsonProperty("name")]
    public string Name => "errorsink";

    [JsonProperty("type")]
    public string Type => "errors";

    [JsonProperty("store")]
    public string Store => "adlsgen2";

    [JsonProperty("cloudType")]
    public string CloudType => "AzurePublic";

    [JsonProperty("endpointSuffix")]
    public string EndpointSuffix => "storage.azure.net";

    [JsonProperty("account")]
    public string Account { get; set; }

    [JsonProperty("fileSystem")]
    public string FileSystem { get; set; }

    [JsonProperty("folderPath")]
    public string FolderPath { get; set; }

    [JsonProperty("format")]
    public string Format => "parquet";

    [JsonProperty("sasToken")]
    public string SasToken { get; set; } = string.Empty;

    [JsonProperty("dnsZone")]
    public string DnsZone { get; set; }
}
