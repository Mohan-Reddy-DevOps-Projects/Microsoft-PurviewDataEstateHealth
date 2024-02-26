namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum DHControlStatus
{
    Enabled,
    Disabled
}
