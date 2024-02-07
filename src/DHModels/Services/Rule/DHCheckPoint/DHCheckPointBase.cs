namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHCheckPoint;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHCheckPoints
{
    Score,
    DataProductDescriptionContent,
    DataProductDescriptionLength,
}
