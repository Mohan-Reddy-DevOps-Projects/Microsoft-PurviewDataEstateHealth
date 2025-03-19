namespace DEH.Domain.Backfill.Catalog;

using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum EventSource
{
    DataCatalog
}

[JsonConverter(typeof(StringEnumConverter))]
public enum PayloadKind
{
    BusinessDomain,
    DataProduct,
    DataAsset,
    CriticalDataElement,
    Relationship,
    Term,
    OKR,
    KeyResult,
    CustomMetadata
}

[JsonConverter(typeof(StringEnumConverter))]
public enum OperationType
{
    Create,
    Update
}