namespace Microsoft.Purview.DataEstateHealth.DHModels.Models;

using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Newtonsoft.Json;
using System.Collections.Generic;

public class JobSubmitPayload
{
    public JobSubmitPayload()
    {
        this.DatasetToDatasourceMappings = new List<DatasetToDatasourceMapping>
        {
            new DatasetToDatasourceMapping()
        };
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
        string sasToken,
        // e.g. https://dgprocessingwus2cyqgjoc.z28.blob.storage.azure.net
        string storageEndpoint,
        string accountId,
        string businessDomainId,
        string dataProductId,
        string dataAssetId,
        string jobId)
    {
        this.SasToken = sasToken;

        var tmp = storageEndpoint.Substring("https://".Length);
        var splited = tmp.Split(".");
        this.Account = splited[0];
        this.DnsZone = splited[1];

        this.FileSystem = accountId;
        this.FolderPath = $"all-errors/businessDomain={businessDomainId}/dataProduct={dataProductId}/dataAsset={dataAssetId}/observation={jobId}";
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
    public string EndpointSuffix => "core.windows.net";

    [JsonProperty("account")]
    public string Account { get; set; }

    [JsonProperty("fileSystem")]
    public string FileSystem { get; set; }

    [JsonProperty("folderPath")]
    public string FolderPath { get; set; }

    [JsonProperty("format")]
    public string Format => "parquet";

    [JsonProperty("sasToken")]
    public string SasToken { get; set; }

    [JsonProperty("dnsZone")]
    public string DnsZone { get; set; }
}
